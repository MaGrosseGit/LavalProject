//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Callbacks;

/// <summary>
/// Editor class for deployment of the Tobii EyeX Client library and .NET binding
/// to where they need to be. 
/// </summary>
[InitializeOnLoad]
public class EyeXClientLibraryDeployer
{
    private const string ClientLibraryFileName = "Tobii.EyeX.Client.dll";
    private const string ClientLibraryDotNetBindingFileName = "Tobii.EyeX.Client.Net20.dll";

    private const string Source32BitDirectory = "Assets/Standard Assets/EyeXFramework/lib/x86/";
    private const string Source64BitDirectory = "Assets/Standard Assets/EyeXFramework/lib/x64/";
    private const string PluginsDirectory = "Assets/Plugins/";

    /// <summary>
    /// When loading the editor, copy the correct version of the EyeX client 
    /// library to the project root folder to be able to run in the editor.
    /// </summary>
    static EyeXClientLibraryDeployer()
    {
        var targetClientLibraryPath = Path.Combine(Directory.GetCurrentDirectory(), ClientLibraryFileName);

        // TODO: When Unity 5 is released, check for 32-bit or 64-bit editor
        if (!File.Exists(targetClientLibraryPath))
        {
            Copy32BitClientLibrary(targetClientLibraryPath);
        }

        var pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), PluginsDirectory);
        if (!Directory.Exists(pluginsPath))
        {
            Directory.CreateDirectory(pluginsPath);
        }
        
        if (!File.Exists(Path.Combine(PluginsDirectory, ClientLibraryDotNetBindingFileName)))
        {
            Copy32BitDotNetBinding();
        }
    }

    /// <summary>
    /// After a build, copy the correct EyeX client library to the output directory. 
    /// </summary>
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        var targetClientLibraryPath = Path.Combine(Path.GetDirectoryName(pathToBuiltProject), ClientLibraryFileName);

        if (target == BuildTarget.StandaloneWindows)
        {
            Copy32BitClientLibrary(targetClientLibraryPath);
        }
        else if (target == BuildTarget.StandaloneWindows64)
        {
            Copy64BitClientLibrary(targetClientLibraryPath);
        }
        else
        {
            Debug.LogWarning("The Tobii EyeX Framework for Unity is only compatible with Windows Standalone builds.");
        }
    }

    private static void Copy32BitClientLibrary(string targetClientDllPath)
    {
        File.Copy(Path.Combine(Source32BitDirectory, ClientLibraryFileName), targetClientDllPath, true);
    }

    private static void Copy64BitClientLibrary(string targetClientDllPath)
    {
        File.Copy(Path.Combine(Source64BitDirectory, ClientLibraryFileName), targetClientDllPath, true);
    }

    private static void Copy32BitDotNetBinding()
    {
        File.Copy(Path.Combine(Source32BitDirectory, ClientLibraryDotNetBindingFileName), 
            Path.Combine(PluginsDirectory, ClientLibraryDotNetBindingFileName), true);
    }

    private static void Copy64BitDotNetBinding()
    {
        File.Copy(Path.Combine(Source64BitDirectory, ClientLibraryDotNetBindingFileName), 
            Path.Combine(PluginsDirectory, ClientLibraryDotNetBindingFileName), true);
    }
}