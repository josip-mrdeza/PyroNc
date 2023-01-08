 using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
 using System.Reflection;
 using Microsoft.Win32.SafeHandles;

 namespace Pyro.IO.Memory
{
    public class SharedMemory
    {
        public string Id;
        public long Capacity;
        public string Path;
        public FileStream Stream;
        public MemoryMappedFile MappedFile;
        private MemoryMappedViewAccessor View;
        private Dictionary<string, Dictionary<string, long>> Offsets;
        private Dictionary<string, MemoryMappedViewAccessor> Views;
        private Dictionary<string, long> ViewOffsets;
        private Dictionary<string, long> TSizes;


        public SharedMemory(string id, long maxCapacity, long defaultViewSize = -1)
        {
            Id = id;
            Capacity = maxCapacity;
            Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\MemoryMappedFiles\\{Assembly.GetCallingAssembly().GetName().Name}";
            Directory.CreateDirectory(Path);
            Path += $"\\{Id}.mmf";
            if (File.Exists(Path))
            {
                Stream = new FileStream(Path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            else
            {
                Stream = File.Create(Path);
            }
            WriteProcessIdToMMFFolder(Stream.SafeFileHandle);
            MappedFile = MemoryMappedFile.CreateFromFile(Stream, id, maxCapacity, MemoryMappedFileAccess.ReadWrite,
                                                             HandleInheritability.Inheritable, true);
            Offsets = new Dictionary<string, Dictionary<string, long>>();
            Views = new Dictionary<string, MemoryMappedViewAccessor>();
            ViewOffsets = new Dictionary<string, long>();
            if (defaultViewSize == -1)
            {
                defaultViewSize = maxCapacity;
            }
            View = CreateNewSection_Internal("default", defaultViewSize);
            TSizes = new Dictionary<string, long>();
        }

        internal void WriteProcessIdToMMFFolder(SafeFileHandle handle)
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\MemoryMappedFiles\\Processes";
            var file = $"{dir}\\{Id}.mmf.process";
            File.WriteAllText(file, handle.DangerousGetHandle().ToInt32().ToString());
        }

        internal SafeFileHandle ReadProcessIdTomMFFolder()
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\MemoryMappedFiles\\Processes";
            var file = $"{dir}\\{Id}.mmf.process";
            var ptr = File.ReadAllText(file);
            var sfh = new SafeFileHandle(new IntPtr(int.Parse(ptr)), false);

            return sfh;
        }
        
        public long GetOffsetForView(string viewId)
        {
            return ViewOffsets[viewId];
        }

        public long GetOffsetForStructInView(string viewId, string structureId)
        {
            return Offsets[viewId][structureId];
        }

        public long GetAvailableOffsetForStructInView(string viewId)
        {
            return Offsets[viewId].LastOrDefault().Value;
        }
        
        public void CreateNewSection(string id, long capacity)
        {
            CreateNewSection_Internal(id, capacity);
        }
        
        private MemoryMappedViewAccessor CreateNewSection_Internal(string id, long capacity)
        {
            var offset = ViewOffsets.LastOrDefault().Value;
            var view = MappedFile.CreateViewAccessor(offset, capacity);
            Views.Add(id, view);
            ViewOffsets.Add(id, offset + capacity);
            Offsets.Add(id, new Dictionary<string, long>());
            return view;
        }
        /// <summary>
        /// Writes data to the part of the memory that applies to a select view.
        /// </summary>
        /// <param name="structure">The structure value.</param>
        /// <param name="viewId">The id of a view created previously, if none were created explicitly, viewId = "default".</param>
        /// <param name="structUniqueId">A unique identifier for this structure type value.</param>
        /// <param name="size">The size of the structure type.</param>
        /// <typeparam name="T">A structure type.</typeparam>
        public void Write<T>(T structure, string viewId = null, string structUniqueId = null) where T : unmanaged
        {
            var dft = typeof(T).Name;
            if (viewId == null)
            {
                viewId = "default";
            }
            var view = Views[viewId];
            if (view.CanWrite)
            {
                var offset = GetAvailableOffsetForStructInView(viewId);
                view.Write(offset, ref structure);
                if (structUniqueId == null)
                {
                    structUniqueId = dft;
                }

                int size;
                unsafe
                {
                    size = sizeof(T);
                }

                if (!Offsets[viewId].ContainsKey(structUniqueId))
                {
                    Offsets[viewId].Add(structUniqueId, offset + size);
                }
                if (!TSizes.ContainsKey(structUniqueId))
                {
                    TSizes.Add(structUniqueId, size);
                }
            }
        }
        /// <summary>
        /// Writes data to the part of the memory that applies to a select view.
        /// </summary>
        /// <param name="structureArr">The structure array value.</param>
        /// <param name="viewId">The id of a view created previously, if none were created explicitly, viewId = "default".</param>
        /// <param name="structUniqueId">A unique identifier for this structure type value.</param>
        /// <param name="sizeOfEach">The size of the structure type.</param>
        /// <typeparam name="T">A structure type.</typeparam>
        public void Write<T>(T[] structureArr, string viewId = null, string structUniqueId = null) where T : unmanaged
        {
            var dft = "default";
            if (viewId == null)
            {
                viewId = dft;
            }
            var view = Views[viewId];
            if (view.CanWrite)
            {
                var offset = GetAvailableOffsetForStructInView(viewId);
                view.WriteArray(offset, structureArr, 0, structureArr.Length);
                if (structUniqueId == null)
                {
                    structUniqueId = dft;
                }                    

                int sizeOfEach;
                unsafe
                {
                    sizeOfEach = sizeof(T);
                }
                Offsets[viewId].Add(structUniqueId, offset + (structureArr.Length * sizeOfEach));
                if (!TSizes.ContainsKey(structUniqueId))
                {
                    TSizes.Add(structUniqueId, sizeOfEach);
                }
            }
        }
        /// <summary>
        /// Reads a byte[] from the set view, from offset
        /// </summary>
        /// <param name="viewId">The name of the view.</param>
        /// <param name="structureUniqueId">The name of the structure type in the private offset dictionary.</param>
        /// <returns>A reference to an array of structures.</returns>
        public byte[] Read(string viewId, string structureUniqueId)
        {
            var view = Views[viewId];
            if (view.CanRead)
            {
                var elementsInArray = (int) Offsets[viewId][structureUniqueId];
                byte[] arr = new byte[elementsInArray];
                var offset = GetOffsetForStructInView(viewId, structureUniqueId);
                view.ReadArray(offset - elementsInArray, arr, 0, elementsInArray);
                if (!TSizes.ContainsKey(structureUniqueId))
                {
                    TSizes.Add(structureUniqueId, 1);
                }

                return arr;
            }

            return default;
        }
        /// <summary>
        /// Reads a structure from the set view, from offset
        /// </summary>
        /// <param name="viewId">The name of the view.</param>
        /// <param name="structureUniqueId">The name of the structure type in the private offset dictionary.</param>
        /// <typeparam name="T">A type of structure.</typeparam>
        /// <returns>A reference to an array of structures.</returns>
        public T Read<T>(string viewId, string structureUniqueId) where T : struct
        {
            var view = Views[viewId];
            
            if (view.CanRead)
            {
                var offset = GetOffsetForStructInView(viewId, structureUniqueId);
                var reduce = TSizes[structureUniqueId];
                view.Read(offset - reduce, out T val);

                return val;
            }

            return default;
        }
        
        /// <summary>
        /// Reads a structure from the set view, from offset
        /// </summary>
        /// <typeparam name="T">A type of structure.</typeparam>
        /// <returns>A reference to an array of structures.</returns>
        public T Read<T>(long offset = 0) where T : unmanaged
        {
            var view = Views["default"];
            
            if (view.CanRead)
            {
                view.Read(offset, out T val);

                return val;
            }

            return default;
        }

        /// <summary>
        /// Reads a structure from the set view, from offset
        /// </summary>
        /// <typeparam name="T">A type of structure.</typeparam>
        /// <returns>A reference to an array of structures.</returns>
        public T Read<T>(string viewId, long offset) where T : unmanaged
        {
            var view = Views[viewId];
            
            if (view.CanRead)
            {
                view.Read(offset, out T val);

                return val;
            }

            return default;
        }

        /// <summary>
        /// Reads a structure array from the set view, from offset til (offset + elementsInArray * sizeOfEach)
        /// </summary>
        /// <param name="viewId">The name of the view.</param>
        /// <param name="structureUniqueId">The name of the structure type in the private offset dictionary.</param>
        /// <param name="elementsInArray">The size of the array.</param>
        /// <param name="sizeOfEach">The size of each structure in the array.</param>
        /// <typeparam name="T">A type of structure.</typeparam>
        /// <returns>A reference to an array of structures.</returns>
        public T[] Read<T>(string viewId, string structureUniqueId, int elementsInArray, int sizeOfEach) where T : unmanaged
        {
            var view = Views[viewId];
            if (view.CanRead)
            {
                T[] arr = new T[elementsInArray];
                var offset = GetOffsetForStructInView(viewId, structureUniqueId);
                view.ReadArray(offset - (elementsInArray * sizeOfEach), arr, 0, elementsInArray * sizeOfEach);

                return arr;
            }

            return null;
        }
        
        /// <summary>
        /// Reads a structure array from the set view, from offset til (offset + elementsInArray * sizeOfEach)
        /// </summary>
        /// <param name="viewId">The name of the view.</param>
        /// <param name="structureUniqueId">The name of the structure type in the private offset dictionary.</param>
        /// <param name="elementsInArray">The size of the array.</param>
        /// <typeparam name="T">A type of structure.</typeparam>
        /// <returns>A reference to an array of structures.</returns>
        public T[] Read<T>(string viewId, string structureUniqueId, int elementsInArray) where T : unmanaged
        {
            var view = Views[viewId];
            if (view.CanRead)
            {
                T[] arr = new T[elementsInArray];
                var offset = GetOffsetForStructInView(viewId, structureUniqueId);
                var reduce = TSizes[structureUniqueId];
                var count = (elementsInArray * reduce);
                var pos = offset - count;
                view.ReadArray(pos, arr, 0, elementsInArray);

                return arr;
            }

            return null;
        }

        /// <summary>
        /// Reads a structure array from the set view, from offset til (offset + elementsInArray)
        /// </summary>
        /// <param name="viewId">The name of the view.</param>
        /// <param name="offset">The offset from the beginning of the MMF.</param>
        /// <param name="elementsInArray">The size of the array.</param>
        /// <param name="sizeOfEach">The size of each struct in an array.</param>
        /// <typeparam name="T">A type of structure.</typeparam>
        /// <returns>A reference to an array of structures.</returns>
        public T[] Read<T>(string viewId, long offset, int elementsInArray, int sizeOfEach) where T : unmanaged
        {
            var view = Views[viewId];
            if (view.CanRead)
            {
                T[] arr = new T[elementsInArray];
                view.ReadArray(offset, arr, 0, elementsInArray * sizeOfEach);

                return arr;
            }

            return null;
        }

        /// <summary>
        /// Reads a structure from the default view, from offset til (offset + (elementsInArray * sizeOfEach));
        /// </summary>
        /// <param name="offset">The offset from the beginning of the MMF.</param>
        /// <param name="elementsInArray">The size of the array.</param>
        /// <typeparam name="T">A type of structure.</typeparam>
        /// <returns>A reference to an array of structures.</returns>
        public T[] Read<T>(long offset, int elementsInArray) where T : unmanaged
        {
            var view = Views["default"];
            if (view.CanRead)
            {
                T[] arr = new T[elementsInArray];
                view.ReadArray(offset, arr, 0, elementsInArray);

                return arr;
            }

            return null;
        }

        ~SharedMemory()
        {
            MappedFile.Dispose();
        }
    }
}