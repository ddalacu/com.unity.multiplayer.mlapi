![](https://i.imgur.com/d0amtqs.png)

MLAPI (Mid level API) is a framework that hopefully simplifies building networked games in Unity. It is built on the LLAPI and is similar to the HLAPI in many ways. It does not however integrate into the compiler and it's meant to offer much greater flexibility than the HLAPI while keeping some of it's simplicity. It offers greater performance over the HLAPI.

[![Github All Releases](https://img.shields.io/github/downloads/MidLevel/MLAPI/total.svg)](https://github.com/MidLevel/MLAPI/releases)
[![GitHub Release](https://img.shields.io/github/release/MidLevel/MLAPI.svg)](https://github.com/MidLevel/MLAPI/releases)
[![Build status](https://ci.appveyor.com/api/projects/status/isxpxba8r76x7chu/branch/master?svg=true)](https://ci.appveyor.com/project/MidLevel/mlapi/branch/master)
[![Codecov](https://codecov.io/gh/MidLevel/MLAPI/branch/master/graph/badge.svg)](https://codecov.io/gh/MidLevel/MLAPI)
[![AppVeyor tests branch](https://img.shields.io/appveyor/tests/MidLevel/MLAPI/master.svg)](https://ci.appveyor.com/project/MidLevel/mlapi/build/tests)
[![Discord](https://img.shields.io/discord/449263083769036810.svg)](https://discord.gg/FM8SE9E)


[![Licence](https://img.shields.io/github/license/MidLevel/MLAPI.svg)](https://github.com/MidLevel/MLAPI/blob/master/LICENCE)
[![Wiki](https://img.shields.io/badge/docs-wiki-green.svg)](https://github.com/MidLevel/MLAPI/wiki)
[![API](https://img.shields.io/badge/docs-api-green.svg)](https://MidLevel.github.io/MLAPI/docs/index.html)

### Documentation
To get started, check the [Wiki](https://github.com/MidLevel/MLAPI/wiki).
This is also where most documentation lies.

To get the latest features, the CI server automatically builds the latest commits from master branch. Note that this build still requires the other DLL's. It might be unstable. You can download it [Here](https://ci.appveyor.com/project/MidLevel/mlapi/build/artifacts)

There is also a autogenerated Sandcastle [API reference](https://MidLevel.github.io/MLAPI/docs/index.html).

### Support
For bug reports or feature requests you want to propose, please use the Issue Tracker on GitHub. For general questions, networking advice or to discuss changes before proposing them, please use the [Discord server](https://discord.gg/FM8SE9E).

### Requirements
* Unity 2017 or newer
* .NET 4.6 or .NET 3.5 with .NET 2.0 non subset [Issue](https://github.com/MidLevel/MLAPI/issues/43)

## Feature highlights
* Host support (Client hosts the server)
* Object and player spawning \[[Wiki page](https://github.com/MidLevel/MLAPI/wiki/Object-Spawning)\]
* Connection approval \[[Wiki page](https://github.com/MidLevel/MLAPI/wiki/Connection-Approval)\]
* Strongly Typed RPC Messaging \[[Wiki page](https://github.com/MidLevel/MLAPI/wiki/Message-System)\]
* Replace the integer QOS with names. When you setup the networking you specify names that are associated with a channel. This makes it easier to manage. You can thus specify that a message should be sent on the "damage" channel which handles all damage related logic and is running on the AllCostDelivery channel.
* ProtocolVersion to allow making different versions not talk to each other.
* NetworkedBehaviours does not have to be on the root, it's simply just a class that implements the send methods etc.
* Custom tickrate
* Synced network time
* Supports separate Unity projects crosstalking
* Scene Management \[[Wiki page](https://github.com/MidLevel/MLAPI/wiki/Scene-Management)\]
* Built in Lag compensation \[[Wiki page](https://github.com/MidLevel/MLAPI/wiki/Lag-Compensation)\]
* NetworkTransform replacement \[[Wiki page](https://github.com/MidLevel/MLAPI/wiki/NetworkedTransform)\]
* Port of NetworkedAnimator \[[Wiki page](https://github.com/MidLevel/MLAPI/wiki/NetworkedAnimator)\]
* Networked NavMeshAgent \[[Wiki page](https://github.com/MidLevel/MLAPI/wiki/NetworkedNavMeshAgent)\]
* Networked Object Pooling \[[Wiki page](https://github.com/MidLevel/MLAPI/wiki/Networked-Object-Pooling)\]
* Networked Vars \[[Wiki page](https://github.com/MidLevel/MLAPI/wiki/NetworkedVar)\]
* Encryption \[[Wiki page](https://github.com/MidLevel/MLAPI/wiki/Message-Encryption)\]
* Super efficient BitWriter & BitReader \[[Wiki page](https://github.com/MidLevel/MLAPI/wiki/BitWriter-&-BitReader)\]
* Custom UDP transport support \[[Wiki page](https://github.com/MidLevel/MLAPI/wiki/Custom-Transports)\]
* NetworkProfiler \[[Wiki page](https://github.com/MidLevel/MLAPI/wiki/NetworkProfiler-Editor-Window)\]

## Special thanks
Special thanks to [Gabriel Tofvesson](https://github.com/GabrielTofvesson) for writing the BitWriter, BitReader & ECDH implementation

## Issues and missing features
If there are any issues, bugs or features that are missing. Please open an issue on the GitHub [issues page](https://github.com/MidLevel/MLAPI/issues)

## Example
[Example project](https://github.com/MidLevel/MLAPI-Examples)

The example project has a much lower priority compared to the library itself. If something doesn't exist in the example nor the wiki. Please open an issue on GitHub.


### Sample Chat
Here is a sample MonoBehaviour showing a chat script where everyone can write and read from.

```csharp
public class Chat : NetworkedBehaviour
{
    private NetworkedList<string> ChatMessages = new NetworkedList<string>(new MLAPI.NetworkedVar.NetworkedVarSettings()
    {
        ReadPermission = MLAPI.NetworkedVar.NetworkedVarPermission.Everyone,
        WritePermission = MLAPI.NetworkedVar.NetworkedVarPermission.Everyone,
        SendTickrate = 5
    }, new List<string>());

    private string textField = "";

	private void OnGUI()
    {
		if (isClient)
        {
            textField = GUILayout.TextField(textField, GUILayout.Width(200));
            if (GUILayout.Button("Send") && !string.IsNullOrWhiteSpace(textField))
            {
                ChatMessages.Add(textField);
                textField = "";
            }

            for (int i = ChatMessages.Count - 1; i >= 0; i--)
            {
                GUILayout.Label(ChatMessages[i]);
            }
        }
	}
}
```