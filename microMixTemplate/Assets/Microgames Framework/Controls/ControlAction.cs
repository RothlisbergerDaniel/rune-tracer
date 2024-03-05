using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControlAction : short
{
    None,    

    Button1 = 1,
    Button1Release = Button1 | JoystickUp,
    Button1Mash = Button1 | JoystickUp | JoystickDown,
    Button1Hold = Button1 | JoystickDown,

    Button2 = 2,
    Button2Release = Button2 | JoystickUp,
    Button2Mash = Button2 | JoystickUp | JoystickDown,
    Button2Hold = Button2 | JoystickDown,

    Joystick = 4,

    JoystickUp = 8,
    JoystickDown = 16,
    JoystickLeft = 32,
    JoystickRight = 64,

    JoystickCW = Joystick | JoystickUp | JoystickRight,
    JoystickCCW = Joystick | JoystickUp | JoystickLeft,

    JoystickVertical = Joystick | JoystickUp | JoystickDown,
    JoystickHorizontal = Joystick | JoystickLeft | JoystickRight,
    JoystickAllDirections = Joystick | JoystickUp | JoystickDown | JoystickLeft | JoystickRight,
}
