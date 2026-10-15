# Scentient Unity API

The Scentient Unity API lets your Unity app control an Escents scent device. Your app connects to the device over Bluetooth, releases scents at the right moments, and can read information such as which scents are loaded and how much battery is left.

This guide covers version 0.1.7 of the package. Last updated 15 October 2026.

Questions or problems? Email us at [team@scentient.tech](mailto:team@scentient.tech).

## Contents

- [Before you start](#before-you-start)
- [Getting started with the VR demo](#getting-started-with-the-vr-demo)
- [Add scents to your own scene without code](#add-scents-to-your-own-scene-without-code)
- [Using the API from code](#using-the-api-from-code)
- [How it works](#how-it-works)
- [Managing connections](#managing-connections)
- [Emitting scents](#emitting-scents)
- [Reading information from the device](#reading-information-from-the-device)
- [Scent list](#scent-list)
- [Troubleshooting](#troubleshooting)
- [API at a glance](#api-at-a-glance)
- [Getting help](#getting-help)

## Before you start

You will need:

- **An Escents device**, charged and switched on, with scent pods inserted.
- **A Meta Quest headset** to run the VR demo. We have tested with a Meta Quest 2. The API also works on Android phones and tablets, which we have tested with a OnePlus Nord.
- **Unity 2021.3.18 or newer**, with the **Android Build Support** module installed.
- **An internet connection the first time your app connects**, so it can download the list of scent names. After that, the app keeps a copy and works offline.

> **Note:** The API only works in Android builds. You cannot connect to an Escents device from the Unity Editor, so test on your headset or Android device.

## Getting started with the VR demo

The quickest way to try Scentient is the VR demo scene that comes with the package. It shows the main features working together:

- scanning for and connecting to an Escents device
- releasing a scent when you pick up an object
- releasing a scent when you walk into an area

You do not need to write any code. Setting up takes about 30 minutes.

> **Making a phone app instead?** Skip step 1, then in step 4 import the **Scentient Connect And Control Example** on its own. It is a phone app for connecting to a device and testing each channel.

### Step 1: Set up a VR project

The demo scene uses the Universal Render Pipeline (URP), OpenXR and the XR Interaction Toolkit.

1. In Unity Hub, create a new project with the **3D (URP)** template. You can also use an existing project that already uses URP.
2. Open **File > Build Settings**, select **Android**, then select **Switch Platform**.
3. Open **Edit > Project Settings > XR Plug-in Management** and select **Install XR Plugin Management** if asked.
4. On the Android tab, tick **OpenXR**, then tick **Meta Quest feature group**.
5. Select **OpenXR** in the left-hand list. On the Android tab, under **Interaction Profiles**, select **+** and add **Oculus Touch Controller Profile**.
6. Open **Window > Package Manager**, select **+**, then **Add package by name**. Enter the name and version below, then select **Add**. If Unity asks to enable the new Input System and restart, select **Yes**.

```
com.unity.xr.interaction.toolkit
2.6.5
```

7. In the Package Manager, select **XR Interaction Toolkit**, open **Samples** and select **Import** next to **Starter Assets**.
8. If **XR Plug-in Management > Project Validation** lists any issues, select **Fix All**.

> **Note:** Use version 2.6.5 of the XR Interaction Toolkit. The demo scene was built with version 2, and its movement controls do not work with version 3.

### Step 2: Install the Scentient packages

The Scentient package needs a separate, open-source Bluetooth library. Install the Bluetooth library first, otherwise the Scentient package will not compile.

1. In the Package Manager, select **+**, then **Add package from git URL**.
2. Paste this URL and select **Add**:

```
https://github.com/paulhayes/Unity-Android-Bluetooth-Low-Energy-Package.git
```

3. Repeat with this URL:

```
https://github.com/ScentientTech/scentient-unity-api.git
```

When it has installed, **Scentient Android API** appears in the Package Manager list.

> **Tip:** Unity installs the latest version of a package from a git URL. To keep everyone on your team on the same version, add `#` and a full commit hash to the end of the URL.

### Step 3: Add the Android permissions

1. Open **Edit > Project Settings > Player**. On the Android tab, expand **Other Settings** and set **Internet Access** to **Require**. This lets your app download the scent names.
2. In **Publishing Settings**, tick **Custom Main Manifest**. Unity creates the file `Assets/Plugins/Android/AndroidManifest.xml`.
3. Open that file and add the lines below inside the `<manifest>` element, just above `<application>`.

```xml
<uses-permission android:name="android.permission.BLUETOOTH" android:maxSdkVersion="30" />
<uses-permission android:name="android.permission.BLUETOOTH_ADMIN" android:maxSdkVersion="30" />
<uses-permission android:name="android.permission.BLUETOOTH_SCAN" android:usesPermissionFlags="neverForLocation" />
<uses-permission android:name="android.permission.BLUETOOTH_CONNECT" />
```

4. To check your work, open **Window > Android Bluetooth Low Energy Library**. This checker comes with the Bluetooth library. Each permission should have a green tick. If it asks which API level you are targeting, choose **API Level 31 or greater**.

If your app needs to run on Android 11 or older, also follow [Android 11 and older](#android-11-and-older).

### Step 4: Import and open the demo scene

1. In the Package Manager, select **Scentient Android API** and open **Samples**.
2. Select **Import** next to **Scentient Connect And Control Example**, then next to **VR Demo Scene**. The VR demo uses a prefab from the Connect and Control example, so you need both.
3. Open `Assets/Samples/Scentient Android API/0.1.7/VR Demo Scene/ScentientDemoScene.unity`.
4. In **File > Build Settings**, select **Add Open Scenes**. Make sure the demo scene is at the top of the list.

### Step 5: Build and run on your headset

1. Turn on developer mode for your headset in the Meta Horizon app on your phone, then connect the headset to your computer with a USB cable.
2. Switch on your Escents device and keep it close to the headset.
3. In **File > Build Settings**, select your headset under **Run Device**, then select **Build And Run**.

### Step 6: Try the demo

1. Put on the headset. If you are asked to allow access to nearby devices, select **Allow**.
2. Find the floating Scentient panel and select **Scan**. When your Escents device appears in the list, select **Connect**.
3. The panel now shows the battery level and a row for each channel, with the scent name, how much is left and a button to release it. Press a channel's button to check it works.
4. **Pick things up.** Grab the hot chocolate, the flower vase or the green fig to release chocolate, flowers or green fig. Grab the shapes on the table to release channels 1 to 3, or press the push button for channel 4.
5. **Walk into an area.** Move to the pine tree or the campfire to release pine or burning wood for 5 seconds.

Objects that use a scent name, such as the hot chocolate, only work if a pod with that scent is in the device. The shapes and the push button use channel numbers, so they work with any pods.

The next time you open the demo, select **Reconnect** to connect to the same device. **Forget** clears the saved device, and **Identify** flashes the blue light on the connected device so you can tell which one it is.

Nothing happened? See [Troubleshooting](#troubleshooting).

## Add scents to your own scene without code

Once the demo works, you can add scents to your own scene with components and the Inspector.

### Add the device and the connection panel

The quickest way is to copy the parts you need from the demo scene.

1. Make sure your scene has an XR rig. If it does not, drag in the **XR Interaction Setup** prefab from `Assets/Samples/XR Interaction Toolkit/2.6.5/Starter Assets/Prefabs`.
2. Open the demo scene. In the Hierarchy, select both **Scentient** and **BleCanvas**, then copy them (**Edit > Copy**).
3. Open your scene and paste them (**Edit > Paste**). Copying both together keeps the links between them.

**Scentient** holds the ScentientDevice component, which manages the connection. **BleCanvas** is the floating panel with the Scan, Connect and Reconnect buttons.

### Release a scent when an object is picked up

1. Select your object. Make sure it has a collider, a Rigidbody and an **XR Grab Interactable** component.
2. Add the **Emit Scent** component.
3. Set **Scent Name** to a name from the [scent list](#scent-list), and set **Duration** (in seconds) and **Intensity** (0 to 1).
4. On the XR Grab Interactable, expand **Interactable Events**. Under **Select Entered**, select **+**, drag your object into the empty field, and choose **EmitScent > Emit**.

**Emit Scent** finds the ScentientDevice in your scene by itself, so you can leave its **Scentient Device** field empty.

To use a channel number instead of a scent name, use the **Emit Scent Channel** component and set **Scent Channel** to a number from 1 to 6. It starts at 0, which is not a valid channel.

> **Note:** The Add Component menu lists two components called Emit Scent Channel. Use the one under **Scripts > Scentient**. The one under **Scripts > Scentient.Samples** belongs to the sample UI.

### Release a scent when the player walks into an area

1. Create an empty GameObject where you want the scent, and add a **Box Collider**. Tick **Is Trigger** and resize it to cover the area.
2. Add a **Rigidbody** and tick **Is Kinematic**.
3. Add the **Trigger Events** and **Emit Scent** components. Set the scent name and a duration of a few seconds.
4. Under **Trigger Enter Event**, add **EmitScent > Emit**. Under **Trigger Exit Event**, add **EmitScent > Stop**.

Some tips for areas:

- **Use a Box, Sphere or Capsule collider with a kinematic Rigidbody.** With a Mesh Collider, or without the Rigidbody, Unity may not report when the player leaves the area.
- **Keep durations short.** Teleporting out of an area may not count as leaving it, so a short duration makes sure the scent ends by itself.
- **Do not overlap areas.** The device releases one scent at a time, so a second area would replace the first area's scent. See [One scent at a time](#one-scent-at-a-time).
- **Trigger Events reacts to anything entering the area.** Use physics layers if you only want the player to set it off.

### Release a scent from a button

Add **Emit Scent** to the button, then under the button's **On Click**, add **EmitScent > Emit**.

## Using the API from code

If you prefer to write code, or want more control, you can use the ScentientDevice component directly. This script connects to an Escents device and releases the scent in channel 1 for two seconds.

1. Create an empty GameObject called `Scentient` and add the **Scentient Device** component.
2. Tick **Verbose** while you are setting up. It writes detailed messages to the log, which helps if something goes wrong. Untick it before you release your app.
3. Create a script called `ScentQuickstart.cs` with the code below, and add it to the same GameObject.
4. Drag the **Scentient Device** component into the script's **Device** field.

```cs
using System.Collections;
using Scentient;
using UnityEngine;

public class ScentQuickstart : MonoBehaviour
{
    [SerializeField] ScentientDevice device;

    IEnumerator Start()
    {
        // Wait one frame so ScentientDevice can finish setting itself up.
        yield return null;

        device.OnStatusChangedEvent += status => Debug.Log($"Scentient: {status}");
        device.OnConnectedEvent.AddListener(OnConnected);

        if (ScentientDevice.HasSavedDevice)
        {
            // Connect to the device this app used last time.
            device.Reconnect();
        }
        else
        {
            // Look for devices and connect to the first one found.
            device.OnDeviceDiscoveredEvent += OnDeviceDiscovered;
            device.Scan();
        }
    }

    void OnDeviceDiscovered(string address, string name)
    {
        device.OnDeviceDiscoveredEvent -= OnDeviceDiscovered;
        device.Connect(address);
    }

    void OnConnected()
    {
        // Release the scent in channel 1 at full intensity for 2 seconds.
        device.SendScentMessage(1, 1f, 2f);
    }
}
```

Build and run on your Android device. After the permission prompt, the app connects and the device releases the scent in channel 1. The API remembers the device, so next time the app reconnects to it.

## How it works

### The pieces

```
Your app or game
    │   for example EmitScent("pine", 0.8f, 3f)
    ▼
ScentientDevice component         this package
    │
    ▼
Android Bluetooth library         installed separately
    │   Bluetooth Low Energy
    ▼
Escents device                    6 channels, one scent pod in each
```

The ScentientDevice component does the work. You add one to your scene and call its methods, either from code or from UnityEvents in the Inspector. It manages the Bluetooth connection, sends your scent commands to the device, and tells your app when something changes, such as the battery level.

### Channels and pods

An Escents device has **6 channels**, numbered 1 to 6. Each channel holds one scent pod. Each pod knows which scent it contains, so the device can tell your app which scent is in each channel when it connects.

### One scent at a time

The device releases **one scent at a time**. Each new command replaces the scent that is playing, even if the new command is for a different channel. See [One scent at a time](#one-scent-at-a-time) for what this means when you design your experience.

### Scent names and IDs

Every scent has a name, such as `pine`, and an ID number, such as `7`. The device stores IDs. The app turns them into names using the [scent list](#scent-list), which it downloads from `https://api.scentient.tech/scent-table_en.csv` when it connects. The app keeps a copy, so names still work offline after the first connection.

### Two ways to release a scent

| Method | Example | Use it when |
|---|---|---|
| By name | `EmitScent("pine", 0.8f, 3f)` | You want your content to work whichever channel the pod is in. |
| By channel | `SendScentMessage(2, 0.8f, 3f)` | You know which pod is in which channel, or you are testing. |

Emitting by name needs the scent list and the device's channel information. Emitting by channel works as soon as you are connected.

### Intensity and duration

- **Intensity** goes from 0 to 1, where 1 is full strength.
- **Duration** is in seconds, up to 60. It must be more than 0, because the device ignores commands with a duration of 0.

## Managing connections

This section explains how the API finds, connects to and remembers an Escents device, and what to do when the connection drops. The connection panel in the VR demo does all of this for you. Read on if you are building your own.

### Connection states

A connection moves through these states. You can read the current state from `State`, or listen for changes with `OnStateChangedEvent`.

| State | What is happening |
|---|---|
| `None` | Not connected. Everything starts and ends here. |
| `Scan` | Looking for nearby Escents devices. |
| `Connect` | Connecting to the chosen device. |
| `Subscribe` | Connected, and reading the device's channels, scent levels and battery. |
| `Ready` | Connected and ready to use. |
| `Unsubscribe` | Disconnecting, because you called `Disconnect()`. |
| `Disconnect` | The connection has closed. Moves to `None` straight after. |

```
None ──► Scan ──► Connect ──► Subscribe ──► Ready
 ▲ ▲      │                                   │
 │ └──────┘                                   │
 │                                            │
 └────────────── Disconnect ◄─────────────────┘
```

- `Scan()` looks for devices, then returns to `None` when the scan ends.
- `Connect()` and `Reconnect()` scan first if they need to, then move to `Connect` as soon as the device is found.
- Whenever the connection closes, for any reason, the state returns to `None`.

The `States` list also contains `ScanRSSI`, `ReadRSSI` and `RequestMTU`. These are not used at the moment.

### Finding devices

Call `Scan()` to look for Escents devices nearby. Each time one is found, `OnDeviceDiscoveredEvent` gives you its address and name. You can use this to build a list for the user to choose from, or connect to the first device found.

```cs
device.OnDeviceDiscoveredEvent += (address, name) =>
{
    Debug.Log($"Found {name} at {address}");
};
device.Scan();
```

- A scan lasts 10 seconds. To change this, set `ScanTimeout` (in seconds) before you call `Scan()`.
- Only devices whose Bluetooth name contains "Escents" are reported.
- If you scan more than once, the same device can be reported again, so check for duplicates if you show a list.

### Connecting

Call `Connect(address)` with an address from `OnDeviceDiscoveredEvent`. If the device has not been found yet in this session, `Connect` scans for it first and connects as soon as it appears.

When the connection is ready, the state changes to `Ready` and `OnConnectedEvent` fires. `OnConnectedEvent` is a UnityEvent, so you can also hook it up in the Inspector.

> **Note:** The device sends its channel information just after `OnConnectedEvent` fires. If you want to emit a scent by name as soon as you connect, see [Emitting a scent as soon as you connect](#emitting-a-scent-as-soon-as-you-connect).

### Remembering a device

When a connection succeeds, the API saves the device's address on the Android device. Next time, call `Reconnect()` to connect to it without asking the user to choose again.

```cs
if (ScentientDevice.HasSavedDevice)
{
    device.Reconnect();
}
```

- `ScentientDevice.HasSavedDevice` tells you whether a device has been saved.
- `ScentientDevice.Forget()` deletes the saved device, for example when the user wants to switch to a different one.
- `Reconnect()` does nothing if no device has been saved.

The address is stored in Unity's `PlayerPrefs`, so calling `PlayerPrefs.DeleteAll()` also forgets the device.

### Disconnecting

Call `Disconnect()` to close the connection. The state moves through `Unsubscribe` and `Disconnect`, then back to `None`. The device stays saved, so you can call `Reconnect()` later.

`Disconnect()` only has an effect once you are connected. It does not cancel a scan or a connection attempt that is in progress.

### When the connection drops

If the Escents device is switched off, runs out of battery or goes out of range, the state moves to `Disconnect` and then `None`. The API does not reconnect by itself.

To reconnect automatically, listen for the `None` state and call `Reconnect()` after a short pause:

```cs
using System.Collections;
using Scentient;
using UnityEngine;

public class StayConnected : MonoBehaviour
{
    [SerializeField] ScentientDevice device;
    [SerializeField] float retryDelaySeconds = 5f;

    // Set this to false before you call Disconnect() yourself.
    public bool keepConnected = true;

    void Start()
    {
        device.OnStateChangedEvent += OnStateChanged;
    }

    void OnDestroy()
    {
        device.OnStateChangedEvent -= OnStateChanged;
    }

    void OnStateChanged(ScentientDevice.States state)
    {
        // None means the connection closed, or a scan ended without finding the device.
        if (state == ScentientDevice.States.None && keepConnected && ScentientDevice.HasSavedDevice)
        {
            StartCoroutine(ReconnectAfterDelay());
        }
    }

    IEnumerator ReconnectAfterDelay()
    {
        yield return new WaitForSeconds(retryDelaySeconds);

        if (keepConnected && device.State == ScentientDevice.States.None)
        {
            device.Reconnect();
        }
    }
}
```

### Connection time limits

A connection attempt does not time out by itself. If your app shows a "Connecting" message, add your own time limit, for example 20 seconds. After that, tell the user to check the device is switched on and nearby, and offer a **Try again** button that calls `Reconnect()`.

### Showing the status to users

`StatusMessage` holds a short description of what is happening, such as "Scanning...", "Connecting..." or "Device Ready". `OnStatusChangedEvent` fires whenever it changes. You can show this text in your UI.

### Permissions

On Android 12 and newer, Android asks the user for permission to find and connect to nearby devices. The API requests this automatically the first time you call `Scan()`, `Connect()` or `Reconnect()`, and carries on once the user allows it.

**If the user denies permission, the app closes by default.** This behaviour comes from the Bluetooth library. To keep your app open instead:

1. Add an **App Permissions** component to a GameObject in your first scene.
2. In its **Permissions** list, expand each entry and untick **Exit On User Denied**.

ScentientDevice uses your App Permissions component instead of adding its own. You can use its **Some Permissions Denied Event** to explain to the user why your app needs Bluetooth.

If you would rather request permissions yourself, untick **Auto Request Permissions On Connect** on the ScentientDevice component.

### Android 11 and older

On Android 6 to 11, Bluetooth scanning only works when your app has location permission and Location is turned on in the device settings. The API does not request location permission for you. If your app only runs on Android 12 or newer, you can skip this section.

1. Add this line to your manifest, next to the Bluetooth permissions:

```xml
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" android:maxSdkVersion="30" />
```

2. Request the permission before you call `Scan()`, `Connect()` or `Reconnect()`:

```cs
// Add "using UnityEngine.Android;" at the top of your script.
if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
{
    Permission.RequestUserPermission(Permission.FineLocation);
}
```

### Apps with more than one scene

Keep the ScentientDevice in a scene that stays loaded while your app runs, and load your other scenes additively (`LoadSceneMode.Additive`). This keeps the Bluetooth connection open when scenes change.

### Good to know

- **Calling from `Start()`:** ScentientDevice sets itself up in its own `Start()`. If your script calls `Scan()`, `Connect()` or `Reconnect()` from `Start()` and happens to run first, the request can be lost. Wait one frame first, as the `ScentQuickstart` script does, or connect from a button.
- **One device at a time:** each ScentientDevice connects to one Escents device. Connecting to several devices at once is not supported.

## Emitting scents

### By name

```cs
device.EmitScent("pine", 0.8f, 3f);   // 80% intensity for 3 seconds
device.EmitScent("pine", 3f);         // full intensity for 3 seconds
```

- Use names from the [scent list](#scent-list). Upper and lower case both work.
- `EmitScent` returns `true` if the command was sent. It returns `false` and logs a warning if the scent is not in the scent list, or if no channel on the device holds it.
- If the same scent is in more than one channel, the lowest channel number is used.

### By channel

```cs
device.SendScentMessage(2, 0.8f, 3f);   // channel 2, 80% intensity, 3 seconds
```

Use a channel number from 1 to 6. This works as soon as you are connected and does not need the scent list.

### One scent at a time

The device releases one scent at a time. Each new command replaces the scent that is playing, even if it is for a different channel. When you design your experience:

- **Stopping stops whatever is playing.** A stop command ends the current scent, whatever started it.
- **Avoid overlapping scent areas.** When the player leaves one area, its stop command also ends the scent of any other area they are still standing in.

### Stopping a scent

To stop the scent that is playing, send a short command with an intensity of 0:

```cs
device.SendScentMessage(1, 0f, 0.1f);   // intensity 0 for 0.1 seconds
```

Because each new command replaces the one before, any channel number works. Do not use a duration of 0, because the device ignores it.

The **Emit Scent** and **Emit Scent Channel** components do this for you when you call **Stop**.

### Emitting a scent as soon as you connect

The device sends its channel information just after you connect. `OnChannelScentsUpdatedEvent` fires once for each channel as that information arrives, and again whenever a channel's scent changes. Wait for it before you emit by name:

```cs
void Start()
{
    device.OnChannelScentsUpdatedEvent += OnChannelScentsUpdated;
}

void OnChannelScentsUpdated(int channel, int scentId, string scentName)
{
    if (scentName == "pine")
    {
        device.EmitScent("pine", 1f, 3f);
    }
}
```

### Scents longer than 60 seconds

One command can last up to 60 seconds. For longer, send the command again before it runs out. Each new command replaces the old one, so the scent carries on without a gap:

```cs
bool keepEmitting = true;

IEnumerator EmitContinuously(string scentName)
{
    while (keepEmitting)
    {
        device.EmitScent(scentName, 1f, 10f);
        yield return new WaitForSeconds(8f);
    }
    device.SendScentMessage(1, 0f, 0.1f);   // stop
}
```

### How often to send commands

Send a command when something changes, such as the player entering an area or picking up an object. Do not send commands every frame, for example from `Update()` or `OnTriggerStay()`. The API sends commands to the device one at a time, so they queue up and arrive late, and each one replaces the scent before it.

## Reading information from the device

### Which scent is in each channel

- `ChannelScentNames` is an array of 6 scent names. Index 0 is channel 1. The entries are empty until the device has sent its channel information.
- `OnChannelScentsUpdatedEvent(channel, scentId, scentName)` fires when that information arrives, and again whenever it changes. `channel` is 1 to 6.
- A channel with scent ID 0 (`nothing`) is empty.

### Scent level

The scent level shows how much scent is left in a pod, from 0 to 100. It is designed to help you keep track of pods during live experiences and shows, so you know when to replace them.

- `GetScentLevel(channel)` returns the level for a channel from 1 to 6.
- `OnChannelScentLevelChangedEvent(channel, level)` fires when the level changes.

The scent level tracks how much scent has been released. It does not account for scent lost in other ways. Scents are volatile, so a pod that is stored incorrectly can dry out much more quickly than its scent level shows.

### Battery

- `Battery` holds the latest battery level, from 0 to 100. It shows 100 until the first reading arrives.
- `OnBatteryLevelChangedEvent(level)` fires when a reading arrives.

### The device button

`OnButtonPressedEvent` fires when someone presses the button on the Escents device. You can use it as an input in your app.

### Identifying a device

`Identify()` flashes the blue light on the connected device, so you can tell which physical unit you are connected to. This helps when there are several devices in the same room.

## Scent list

The API looks up scent names in this list: [scent-table_en.csv](https://api.scentient.tech/scent-table_en.csv). The online file is always the most up to date.

| ID | Name |
|---|---|
| 1 | ocean waves |
| 2 | lavender |
| 3 | flowers |
| 4 | rainforest |
| 5 | waterfall |
| 6 | red berries |
| 7 | pine |
| 8 | chocolate |
| 9 | citrus |
| 10 | cut grass |
| 11 | burning wood |
| 12 | natural gas |
| 13 | petrol |
| 14 | burning plastic |
| 15 | body odour |
| 16 | blood |
| 17 | vomit |
| 18 | cleaning agent |
| 19 | mildew tunnels |
| 20 | green fig |
| 21 | cedar wood |
| 22 | baby powder |
| 23 | old hospital |
| 24 | cigarette smoke |
| 27 | scent1 |
| 28 | scent2 |
| 29 | scent3 |
| 30 | scent4 |
| 31 | scent5 |
| 32 | scent6 |

ID 0, `nothing`, means a channel is empty. IDs 27 to 32, `scent1` to `scent6`, are for custom scents. Use these names to release a custom scent by name.

### Using scent names offline

Your app downloads the scent list each time it connects, and keeps a copy. If the very first connection happens without internet, there is no copy yet, so emitting by name will not work. Emitting by channel still works.

To avoid this, include the list in your app:

1. Download [scent-table_en.csv](https://api.scentient.tech/scent-table_en.csv).
2. Save it in your project as `Assets/Resources/scent-table_en.csv`.

The API uses this file when it cannot download the list and has no saved copy.

## Troubleshooting

First, tick **Verbose** on the ScentientDevice component. Then read the log from your headset or Android device with **Window > Analysis > Android Logcat** in Unity, or by running `adb logcat -s Unity` in a terminal.

### "The type or namespace name 'Android' could not be found"

The Bluetooth library is missing. Install it as described in [step 2](#step-2-install-the-scentient-packages).

### The VR demo has pink objects or missing scripts

- **Pink objects:** your project is not using the Universal Render Pipeline. Start from the **3D (URP)** template.
- **Missing scripts:** import the XR Interaction Toolkit **Starter Assets**, and check you are using XR Interaction Toolkit 2.6.5. See [step 1](#step-1-set-up-a-vr-project).

### Nothing happens in the Unity Editor

The API only works in Android builds. Build and run on your headset or Android device.

### No devices are found

Check that:

- Bluetooth is turned on on the headset or Android device.
- The Escents device is switched on, charged and close by.
- The Escents device is not already connected to another headset or phone.
- The user allowed the permission request. On Android 11 or older, location permission must be granted and Location turned on. See [Android 11 and older](#android-11-and-older).
- The scan runs for long enough. It lasts 10 seconds by default. You can increase `ScanTimeout`.

### Scan(), Connect() or Reconnect() does nothing

- If you call it from `Start()`, wait one frame first. See [Good to know](#good-to-know).
- `Reconnect()` does nothing if no device has been saved. Check `ScentientDevice.HasSavedDevice` first.

### The app closes when the user denies permission

This is the Bluetooth library's default behaviour. See [Permissions](#permissions) to change it.

### The status stays on "Connecting..."

Check the Escents device is switched on and in range, then call `Reconnect()` again. If it keeps happening, switch the Escents device off and on again. See [Connection time limits](#connection-time-limits).

### An object or area does not release a scent

- If it uses a scent name, check that a pod with that scent is in the device. The channel rows on the demo's panel show which scents are loaded.
- Check the log for a warning from `EmitScent`, described below.

### EmitScent returns false

The log tells you why:

- **Not found in scent table:** check the spelling against the [scent list](#scent-list).
- **Not found on device:** no channel holds that scent, or the channel information has not arrived yet. See [Emitting a scent as soon as you connect](#emitting-a-scent-as-soon-as-you-connect).
- **ScentTable has not been loaded:** the app could not download the scent list and has no copy. See [Using scent names offline](#using-scent-names-offline).

### A scent stops sooner than expected

- Another command replaced it. The device releases one scent at a time. See [One scent at a time](#one-scent-at-a-time).
- One command lasts up to 60 seconds. See [Scents longer than 60 seconds](#scents-longer-than-60-seconds).

### A scent does not stop

- Stop with an intensity of 0 and a duration above 0. See [Stopping a scent](#stopping-a-scent).
- If it should stop when the player leaves an area, check the area uses a Box, Sphere or Capsule collider with a kinematic Rigidbody, and keep its duration short. See [Release a scent when the player walks into an area](#release-a-scent-when-the-player-walks-into-an-area).

## API at a glance

This is a summary of the parts you are most likely to use. For full details, see the [ScentientDevice reference](https://api.scentient.tech/class_scentient_1_1_scentient_device.html). Add `using Scentient;` to scripts that use the API.

### Methods

| Method | What it does |
|---|---|
| `Scan()` | Looks for nearby Escents devices. |
| `Connect(string address)` | Connects to a device. Scans for it first if needed. |
| `Reconnect()` | Connects to the last device you connected to. |
| `Disconnect()` | Closes the connection. |
| `ScentientDevice.Forget()` | Deletes the saved device. |
| `EmitScent(string scentName, float duration)` | Releases a scent by name at full intensity. Returns `false` if it could not. |
| `EmitScent(string scentName, float intensity, float duration)` | Releases a scent by name at the intensity you choose. |
| `SendScentMessage(int channel, float intensity, float duration)` | Releases the scent in a channel (1 to 6). |
| `GetScentLevel(int channel)` | Returns how much scent is left in a channel, from 0 to 100. |
| `GetScentDict()` | Returns every ID and name in the scent list. |
| `Identify()` | Flashes the blue light on the device, so you can tell which unit it is. |

### Properties

| Property | What it holds |
|---|---|
| `State` | The current connection state. |
| `StatusMessage` | A short description of the current status. |
| `ChannelScentNames` | The names of the scents in channels 1 to 6. |
| `Battery` | The battery level, from 0 to 100. |
| `ScanTimeout` | How long a scan lasts, in seconds. 10 by default. |
| `Verbose` | Writes detailed messages to the log when `true`. |
| `ScentientDevice.HasSavedDevice` | `true` if a device has been saved. |

### Events

| Event | When it fires |
|---|---|
| `OnStateChangedEvent(States state)` | The connection state changes. |
| `OnStatusChangedEvent(string status)` | The status message changes. |
| `OnDeviceDiscoveredEvent(string address, string name)` | A scan finds an Escents device. |
| `OnConnectedEvent` | The connection is ready. |
| `OnChannelScentsUpdatedEvent(int channel, int scentId, string scentName)` | Channel information arrives or changes. |
| `OnChannelScentLevelChangedEvent(int channel, int level)` | A scent level changes. |
| `OnBatteryLevelChangedEvent(byte level)` | A battery reading arrives. |
| `OnButtonPressedEvent()` | The button on the device is pressed. |

`OnConnectedEvent` is a UnityEvent, so you can set it up in the Inspector. The others are C# events: subscribe with `+=` in code, and unsubscribe with `-=` when your object is destroyed. All events run on Unity's main thread, so you can update your UI from them directly.

### Components

| Component | What it does |
|---|---|
| [ScentientDevice](https://api.scentient.tech/class_scentient_1_1_scentient_device.html) | Connects to the Escents device and controls it. You need one in your scene. |
| [EmitScent](https://api.scentient.tech/class_scentient_1_1_emit_scent.html) | Releases a scent by name when you call `Emit()`, and stops it when you call `Stop()`. |
| [EmitScentChannel](https://api.scentient.tech/class_scentient_1_1_emit_scent_channel.html) | Releases the scent in a channel when you call `Emit()`, and stops it when you call `Stop()`. |
| [TriggerEvents](https://api.scentient.tech/class_scentient_1_1_trigger_events.html) | Runs UnityEvents when something enters or leaves a trigger collider. |

## Getting help

Email us at [team@scentient.tech](mailto:team@scentient.tech). To help us answer quickly, please include:

- your Unity version
- the version of the Scentient package, shown in the Package Manager
- your headset or Android device, and its Android version
- what you expected to happen, and what happened instead
- the log from your device, with **Verbose** turned on
