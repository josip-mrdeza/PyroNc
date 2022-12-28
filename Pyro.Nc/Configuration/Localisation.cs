using System;
using System.Collections.Generic;

namespace Pyro.Nc.Configuration;

public class Localisation
{
    public string ID { get; private set; }
    public Dictionary<MapKey, string> Map { get; private set; }

    public Localisation(string id, Dictionary<MapKey, string> map)
    {
        ID = id;
        Map = map;
    }

    public string Find(MapKey id, params object[] arguments)
    {
        if (Map.ContainsKey(id))
        {
            return Map[id].Format(arguments);
        }

        if (!EnglishMapping.ContainsKey(id))
        {
            throw new NotSupportedException($"English mapping does not contain key '{id}'!");
        }
        return EnglishMapping[id].Format(arguments);
    }
    public static readonly Dictionary<MapKey, string> EnglishMapping = new Dictionary<MapKey, string>()
    {
        {
            MapKey.ConsoleViewCreatedLogStream,
            "Created Log Stream in: {0}."
        },
        {
            MapKey.ConsoleViewAddedLogReceivedHandler,
            "Added handler for Application.logMessageReceived."
        },
        {
            MapKey.ConsoleViewAddedAppQuitHandler,
            "Added handler for Application.quitting."
        },
        {
            MapKey.ConsoleViewQuitting,
            "Quitting..."
        },
        {
            MapKey.ConsoleViewLoggingRuntimeValues,
            "Logging all available runtime values..."
        },
        {
            MapKey.ConsoleViewAssemblyLoad,
            "Found {0} assemblies..."
        },
        {
            MapKey.ConsoleViewTypeLogger,
            "Type '{0}' in assembly '{1}':"
        },
        {
            MapKey.ConsoleViewDisposingLogStream,
            "Disposing log stream in: {0}..."
        },
        {
            MapKey.ConsoleViewFinishSetup,
            "Finished PyroConsoleView Setup!"
        },
        {
            MapKey.StartupCreateManagersFromMemoryCompleted, 
            "Manager '{0}' completed in {1} ms!"
        },
        {
            MapKey.StartupInitializeAsyncQuality,
            "QualityLevel: {0}"
        },
        {
            MapKey.StartupAppInitializing,
            "Application Startup initializing..."
        },
        {
            MapKey.StartupComplete,
            "Startup complete in {0} ms!"
        },
        {
            MapKey.MonoInitializerComplete,
            "Initialized '{0}' in {1} ms!"
        },
        {
            MapKey.MonoInitializerCompleteFull,
            "Completed 'MonoInitializer' in {0} ms!"
        },
        {
            MapKey.DefaultsManagerMissingJson,
            "Missing defaults.json, creating..."
        },
        {
            MapKey.DefaultManagerCreatedMissingJson,
            "DefaultManagerCreatedMissingJson"
        },
        {
            MapKey.CustomAssemblyManagerAddMissingFile,
            "Adding missing file: '{0}' to compiler dir '{1}'." 
        },
        {
            MapKey.CustomAssemblyManagerTitleBrackets,
            "[CustomAssemblyManager]"
        },
        {
            MapKey.CustomAssemblyManagerDirectoryEmpty,
            "[CustomAssemblyManager] - Directory '{0}' is empty, skipping!"
        },
        {
            MapKey.CustomAssemblyManagerAlreadyCompiled,
            "[CustomAssemblyManager] - Directory '{0}' has already been compiled into an assembly, skipping!"
        },
        {
            MapKey.CustomAssemblyManagerStartBuildWithReferences,
            "[CustomAssemblyManager] - Attempting to build assembly with references:"
        },
        {
            MapKey.CustomAssemblyManagerLoadedAssembly,
            "[CustomAssemblyManager] - Loaded assembly '{0}'!"
        },
        {
            MapKey.CustomAssemblyManagerCompletedAndImportedCount,
            "[CustomAssemblyManager] - Completed && Imported {0} assemblies!"
        },
        {
            MapKey.CustomAssemblyManagerCompilerComMessage,
            "[PyroCompiler] - Compiler message: \"{0}\"."
        },
        {
            MapKey.GCodeNoCommandsFound,
            "No commands found in string!"
        },
        {
            MapKey.GCodeFault,
            "Error in GetSuggestions: line -> \"{0}\""
        },
        {
            MapKey.GCodeFaultOrUndeclared,
            "Error in GetSuggestions: line -> \"{0}\""
        },
        {
             MapKey.GenericError, 
             "Generic (Handled?) error: {0}!"
        },
        {
            MapKey.GenericUnhandledError, 
            "Unhandled error: {0}!"
        },
        {
            MapKey.GenericHandledError,
            "Handled error: {0}!"
        },
        {
            MapKey.GCodeNameProgram,
            "Name your program:"
        },
        {
            MapKey.CustomAssemblyManagerFailed,
            "[CustomAssemblyManager] - An error has occured: INFO({0}), {1}!"
        },
        {
            MapKey.GenericMessage,
            "[Message] - {0}"
        }
    };
    
    public enum MapKey
    {
         GenericMessage,
         ConsoleViewCreatedLogStream,
         StartupCreateManagersFromMemoryCompleted,
         StartupInitializeAsyncQuality,
         StartupAppInitializing,
         StartupComplete,
         MonoInitializerComplete,
         MonoInitializerCompleteFull,
         DefaultsManagerMissingJson,
         DefaultManagerCreatedMissingJson,
         CustomAssemblyManagerAddMissingFile,
         CustomAssemblyManagerTitleBrackets,
         CustomAssemblyManagerDirectoryEmpty,
         CustomAssemblyManagerAlreadyCompiled,
         CustomAssemblyManagerStartBuildWithReferences,
         CustomAssemblyManagerLoadedAssembly,
         CustomAssemblyManagerCompletedAndImportedCount,
         CustomAssemblyManagerCompilerComMessage,
         CustomAssemblyManagerFailed,
         ConsoleViewAddedLogReceivedHandler,
         ConsoleViewAddedAppQuitHandler,
         ConsoleViewQuitting,
         ConsoleViewLoggingRuntimeValues,
         ConsoleViewAssemblyLoad,
         ConsoleViewTypeLogger,
         ConsoleViewDisposingLogStream,
         ConsoleViewFinishSetup,
         GCodeNoCommandsFound,
         GCodeFault,
         GenericError,
         GCodeFaultOrUndeclared,
         GCodeNameProgram,
         GenericUnhandledError,
         GenericHandledError
    }
}