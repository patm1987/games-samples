# FPS Demo
This is a demonstration project for implementing mouse capture and
mouse look on an Android device. This project can be built either as
Windows or Android standalone projects and supports ChromeOS and

## Components of Interest
* [MouseCapture](Scripts/MouseCapture) contains the necessary
components to capture a mouse in your own Unity project. This
includes:
  * [InputCapture.java](Scripts/MouseCapture/InputCapture.java) to
capture native Android mouse events.
  * [InputCapture.cs](Scripts/MouseCapture/InputCapture.cs) to move
the mouse data into C#.
  * [MouseCapture.cs](Scripts/MouseCapture/MouseCapture.cs) to make
this aware of Unity frame events and facilitate communicating with
your own `MonoBehaviour`s.

In addition, [Mouselook.cs](Scripts/Mouselook.cs) implements a
mouse-driven camera that can handle both Android and desktop
mouse capture APIs in a unified manner. Including mimicking Unity's
0.1 scaling factor on mouse motion when using the legacy input
system.