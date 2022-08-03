 using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
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


        public SharedMemory(string id, long capacity)
        {
            Id = id;
            Capacity = capacity;
            Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\MemoryMappedFiles\\{Id}.mmf";
            Stream = new FileStream(Path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            WriteProcessIdToMMFFolder(Stream.SafeFileHandle);
            MappedFile = MemoryMappedFile.CreateFromFile(Stream, id, capacity, MemoryMappedFileAccess.ReadWrite,
                                                             HandleInheritability.Inheritable, true);
                Offsets = new Dictionary<string, Dictionary<string, long>>();
            Views = new Dictionary<string, MemoryMappedViewAccessor>();
            ViewOffsets = new Dictionary<string, long>();
            View = CreateNewSection_Internal("default", capacity);
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
        public void Write<T>(T structure, string viewId, string structUniqueId, long size) where T : struct
        {
            var view = Views[viewId];
            if (view.CanWrite)
            {
                var offset = GetAvailableOffsetForStructInView(viewId);
                view.Write(offset, ref structure);
                Offsets[viewId].Add(structUniqueId, offset + size);
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
        public void Write<T>(T[] structureArr, string viewId, string structUniqueId, long sizeOfEach) where T : struct
        {
            var view = Views[viewId];
            if (view.CanWrite)
            {
                var offset = GetAvailableOffsetForStructInView(viewId);
                view.WriteArray(offset, structureArr, 0, structureArr.Length);
                Offsets[viewId].Add(structUniqueId, offset + (structureArr.Length * sizeOfEach));
            }
        }
        /// <summary>
        /// Reads a byte[] from the set view, from offset
        /// </summary>
        /// <param name="viewId">The name of the view.</param>
        /// <param name="structureUniqueId">The name of the structure type in the private offset dictionary.</param>
        /// <returns>A reference to an array of structures.</returns>
        public Reference<byte[]> Read(string viewId, string structureUniqueId)
        {
            var view = Views[viewId];
            if (view.CanRead)
            {
                var elementsInArray = (int) Offsets[viewId][structureUniqueId];
                byte[] arr = new byte[elementsInArray];
                var offset = GetOffsetForStructInView(viewId, structureUniqueId);
                view.ReadArray(offset - elementsInArray, arr, 0, elementsInArray);

                return new Reference<byte[]>(ref arr);
            }

            return null;
        }
        /// <summary>
        /// Reads a structure from the set view, from offset
        /// </summary>
        /// <param name="viewId">The name of the view.</param>
        /// <param name="structureUniqueId">The name of the structure type in the private offset dictionary.</param>
        /// <typeparam name="T">A type of structure.</typeparam>
        /// <returns>A reference to an array of structures.</returns>
        public Reference<T> Read<T>(string viewId, string structureUniqueId) where T : struct
        {
            var view = Views[viewId];
            
            if (view.CanRead)
            {
                var offset = GetOffsetForStructInView(viewId, structureUniqueId);
                view.Read(offset, out T val);

                return new Reference<T>(ref val);
            }

            return null;
        }
        
        /// <summary>
        /// Reads a structure from the set view, from offset
        /// </summary>
        /// <typeparam name="T">A type of structure.</typeparam>
        /// <returns>A reference to an array of structures.</returns>
        public Reference<T> Read<T>(long offset) where T : struct
        {
            var view = Views["default"];
            
            if (view.CanRead)
            {
                view.Read(offset, out T val);

                return new Reference<T>(ref val);
            }

            return null;
        }

        /// <summary>
        /// Reads a structure from the set view, from offset
        /// </summary>
        /// <typeparam name="T">A type of structure.</typeparam>
        /// <returns>A reference to an array of structures.</returns>
        public Reference<T> Read<T>(string viewId, long offset) where T : struct
        {
            var view = Views[viewId];
            
            if (view.CanRead)
            {
                view.Read(offset, out T val);

                return new Reference<T>(ref val);
            }

            return null;
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
        public Reference<T[]> Read<T>(string viewId, string structureUniqueId, int elementsInArray, int sizeOfEach) where T : struct
        {
            var view = Views[viewId];
            if (view.CanRead)
            {
                T[] arr = new T[elementsInArray];
                var offset = GetOffsetForStructInView(viewId, structureUniqueId);
                view.ReadArray(offset, arr, 0, elementsInArray * sizeOfEach);

                return new Reference<T[]>(ref arr);
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
        public Reference<T[]> Read<T>(string viewId, long offset, int elementsInArray, int sizeOfEach) where T : struct
        {
            var view = Views[viewId];
            if (view.CanRead)
            {
                T[] arr = new T[elementsInArray];
                view.ReadArray(offset, arr, 0, elementsInArray * sizeOfEach);

                return new Reference<T[]>(ref arr);
            }

            return null;
        }

        /// <summary>
        /// Reads a structure from the default view, from offset til (offset + (elementsInArray * sizeOfEach));
        /// </summary>
        /// <param name="offset">The offset from the beginning of the MMF.</param>
        /// <param name="elementsInArray">The size of the array.</param>
        /// <param name="sizeOfEach">The size of each struct in an array.</param>
        /// <typeparam name="T">A type of structure.</typeparam>
        /// <returns>A reference to an array of structures.</returns>
        public Reference<T[]> Read<T>(long offset, int elementsInArray, int sizeOfEach) where T : struct
        {
            var view = Views["default"];
            if (view.CanRead)
            {
                T[] arr = new T[elementsInArray];
                view.ReadArray(offset, arr, 0, elementsInArray * sizeOfEach);

                return new Reference<T[]>(ref arr);
            }

            return null;
        }

        ~SharedMemory()
        {
            MappedFile.Dispose();
        }
    }
}