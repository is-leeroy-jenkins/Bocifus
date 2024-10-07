##  _
![](https://github.com/is-leeroy-jenkins/Bocifus/blob/main/Resources/Assets/DemoImages/bocefus.png)
###  A simple application for interacting with the OpenAI API written in C#

![](https://github.com/is-leeroy-jenkins/Bocifus/blob/main/Resources/Assets/DemoImages/OpenAIOnWPF.gif)

## ![](https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/GitHubImages/web.png) Basics

1. Run the application.
2. Click the button next to the Configuration combo box and set the API key.
3. Type your question or comment in the text box and press Ctrl + Enter or click the Send button to submit.
4. The response from the API will appear in the Assistant text box.
5. Continue the dialogue by sending further messages if necessary.

## ![](https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/GitHubImages/system_requirements.png)  Requirements

- You need [VC++ 2019 Runtime](https://aka.ms/vs/17/release/vc_redist.x64.exe) 32-bit and 64-bit versions
- You will need .NET 8.

## ![](https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/GitHubImages/tools.png) Build

- [x] VisualStudio 2022
- [x] Built on  Windows Presentation Framework
- [x] Written in C#
- [x] Windows 10/11

## ![](https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/GitHubImages/documentation.png) Documentation

- [User Guide](Resources/Github/Users.md) - how to use Bocefus.
- [Compilation Guide](Resources/Github/Compilation.md) - instructions on how to compile Bocefus.
- [Configuration Guide](Resources/Github/Configuration.md) - information for the Bocefus configuration file. 
- [Distribution Guide](Resources/Github/Distribution.md) -  distributing Bocefus.


## ![](https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/GitHubImages/csharp.png)  Code

- Bocefus supports 'AnyCPU' as well as x86/x64 specific builds
- [Controls](https://github.com/is-leeroy-jenkins/Bocefus/tree/master/UI/Controls) - controls associated main ui layer and related functionality.
- [Enumerations](https://github.com/is-leeroy-jenkins/Bocefus/tree/master/Enumerations) - various enumerations.
- [Extensions](https://github.com/is-leeroy-jenkins/Bocefus/tree/master/Extensions)- useful extension methods by type.
- [Data](https://github.com/is-leeroy-jenkins/Bocefus/tree/master/Data) - tcp/udp/websockets classses
- [Converters](https://github.com/is-leeroy-jenkins/Bocefus/tree/master/Converters) - type converters 
- [Resources](https://github.com/is-leeroy-jenkins/Bocefus/tree/master/Resources) - statistic classes 
- [Managers](https://github.com/is-leeroy-jenkins/Bocefus/tree/master/Managers) - other tools used and available.
- [Models](https://github.com/is-leeroy-jenkins/Bocefus/tree/master/Model) - models used in network analysis
- [Services](https://github.com/is-leeroy-jenkins/Bocefus/tree/master/Services) - ai services classes used in Bocefus.
- [Static](https://github.com/is-leeroy-jenkins/Bocefus/tree/master/Static) - static types used by Bocefus.
- [Themes](https://github.com/is-leeroy-jenkins/Bocefus/tree/master/UI/Themes) - themes used in the ui
- [Windows](https://github.com/is-leeroy-jenkins/Bocefus/tree/master/UI/Windows) - window classes

## ![](https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/GitHubImages/edit.png)  Dependencies
Here are the key dependencies of this project:
- [**Betalgo.OpenAI** ](https://github.com/betalgo/openai)
- [**ModernWpf**](https://github.com/Kinnara/ModernWpf)
- [**FluentWPF**](https://github.com/sourcechord/FluentWPF)
- [**MdXaml(Fork)**](https://github.com/yt3trees/MdXaml/tree/dev_yt3trees) - This is a forked version from the original project.

## ![](https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/GitHubImages/features.png)  Examples

![](https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/DemoImages/OpenAIOnWPF.gif)

## ![](https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/GitHubImages/baby.png)  Configurations
OpenAI or AzureOpenAI APIs are available.
<img src="https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/DemoImages/57471763/fc457b14-f563-4551-aadb-dffa1c89c316" width="100%" />

## ![](https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/GitHubImages/features.png)  History
https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/DemoImages/57471763/d7bcb773-9832-44cc-8a97-a3a13e6f8c18

## ![](https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/GitHubImages/epa.png)  Prompts
https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/DemoImages/57471763/1057896a-3f72-495c-9db7-68223e5d1136

## ![](https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/GitHubImages/edit.png)  Extras
- For people whose native language is not English.
- English is said to be the most accurate. Use this if you want to interact with the AI in English.
- Requires DeepLearning or Google's translation API.

https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/DemoImages/57471763/18462921-7c50-45d3-87cd-be751a5a3197

### Change Color Theme
![color_theme](https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Resources/Assets/DemoImages/57471763/8ab7f837-e7c6-4038-b98b-20b52146c399)


### NuGet Packages

    '''
    <ItemGroup>
        <PackageReference Include="EPPlus" Version="7.3.2" />
        <PackageReference Include="EPPlus.Interfaces" Version="6.1.1" />
        <PackageReference Include="EPPlus.System.Drawing" Version="6.1.1" />
        <PackageReference Include="Google.Apis.CustomSearchAPI.v1" Version="1.68.0.3520" />
        <PackageReference Include="MahApps.Metro" Version="2.4.10" />
        <PackageReference Include="Microsoft.CSharp" Version="4.7.0" />
        <PackageReference Include="Microsoft.Office.Interop.Excel" Version="15.0.4795.1001" />
        <PackageReference Include="Microsoft.Office.Interop.Outlook" Version="15.0.4797.1004" />
        <PackageReference Include="Microsoft.Office.Interop.Word" Version="15.0.4797.1004" />
        <PackageReference Include="ModernWpfUI" Version="0.9.6" />
        <PackageReference Include="SharpPcap" Version="6.3.0" />
        <PackageReference Include="SkiaSharp" Version="2.88.8" />
        <PackageReference Include="Syncfusion.Licensing" Version="24.1.41" />
        <PackageReference Include="Syncfusion.SfSkinManager.WPF" Version="24.1.41" />
        <PackageReference Include="Syncfusion.Shared.Base" Version="24.1.41" />
        <PackageReference Include="Syncfusion.Shared.WPF" Version="24.1.41" />
        <PackageReference Include="Syncfusion.Themes.FluentDark.WPF" Version="24.1.41" />
        <PackageReference Include="Syncfusion.Tools.WPF" Version="24.1.41" />
        <PackageReference Include="Syncfusion.UI.WPF.NET" Version="27.1.48" />
        <PackageReference Include="System.Data.Common" Version="4.3.0" />
        <PackageReference Include="System.Data.OleDb" Version="8.0.0" />
        <PackageReference Include="System.Data.SqlClient" Version="4.8.6" />
        <PackageReference Include="ToastNotifications.Messages.Net6" Version="1.0.4" />
        <PackageReference Include="ToastNotifications.Net6" Version="1.0.4" />
      </ItemGroup>
	    
### Assebly References

        <Reference Include="System.Data.SQLite">
            <HintPath>Libraries\System.Data\System.Data.SQLite.dll</HintPath>
        </Reference>
        <Reference Include="System.Data.SQLite.EF6">
            <HintPath>Libraries\System.Data\System.Data.SQLite.EF6.dll</HintPath>
        </Reference>
        <Reference Include="System.Data.SQLite.Linq">
            <HintPath>Libraries\System.Data\System.Data.SQLite.Linq.dll</HintPath>
        </Reference>
        <Reference Include="System.Data.SqlServerCe">
            <HintPath>Libraries\System.Data\System.Data.SqlServerCe.dll</HintPath>
        </Reference>
      '''

### Author

[Terry Eppler](mailto://terry.eppler@gmail.com)

### License

[MIT](https://github.com/is-leeroy-jenkins/Bocefus/blob/main/Properties/LICENSE)
