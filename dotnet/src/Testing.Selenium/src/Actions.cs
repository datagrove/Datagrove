namespace Datagrove.Testing.Selenium;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using Microsoft.Playwright;
// we can have lambda's instead of actions.


public interface ILocatable
{
    Point LocationOnScreenOnceScrolledIntoView { get; }
    ICoordinates Coordinates { get; }
}
public enum CoordinateOrigin
{
    Viewport, Pointer, Element
}
public enum PointerKind
{
    Mouse, Pen, Touch
}

public enum MouseButton
{
    Touch = 0, Left = 0, Middle = 1, Right = 2, Back = 3, Forward = 4,
}

public enum InputDeviceKind
{
    None, Key, Pointer, Wheel
}
public interface ICoordinates
{
    Point LocationOnScreen { get; }

    Point LocationInViewport { get; }

    Point LocationInDom { get; }

    object AuxiliaryLocator { get; }
}

public class Actions
{
    PlaywrightDriver driver;
    List<Func<PlaywrightDriver, Task<object>>> actions = new();

    private readonly TimeSpan DefaultScrollDuration = TimeSpan.FromMilliseconds(250);
    private readonly TimeSpan DefaultMouseMoveDuration = TimeSpan.FromMilliseconds(250);
    private PointerInputDevice defaultMouse = new(PointerKind.Mouse, "default mouse");
    private KeyInputDevice defaultKeyboard = new("default keyboard");
    private WheelInputDevice defaultWheel = new("default wheel");

    public void Perform()
    {
        // note that when we do this in javascript, we can't send lambdas to a worker.
        driver.Perform(actions);
    }

    public Actions(IWebDriver driver)
    {
        this.driver = (PlaywrightDriver)driver;
    }

    public Actions KeyDown(string theKey)
    {
        throw new NotImplementedException();

    }
    public Actions KeyDown(IWebElement element, string theKey)
    {
        throw new NotImplementedException();

    }

    public Actions KeyUp(string theKey)
    {
        throw new NotImplementedException();
 
    }

    public Actions KeyUp(IWebElement element, string theKey)
    {
        throw new NotImplementedException();

    }

    public Actions SendKeys(string keysToSend)
    {
        actions.Add(async (proxy) =>
        {
            await proxy.page.Keyboard.TypeAsync(keysToSend);
            return true;
        });

        return this;
    }

    public Actions SendKeys(IWebElement element, string keysToSend)
    {
        var e = (PWebElement)element;
        actions.Add(async (proxy) =>
        {
            await e.h.FillAsync(keysToSend);
            await proxy.page.Keyboard.TypeAsync(keysToSend);
            return true;
        });
        return this;
    }


    public Actions ClickAndHold(IWebElement onElement)
    {
        throw new NotImplementedException(); throw new NotImplementedException();
      
    }

    public Actions ClickAndHold()
    {
        throw new NotImplementedException();
       
    }

    public Actions Release(IWebElement onElement)
    {
        throw new NotImplementedException();
       
    }

    public Actions Release()
    {
        throw new NotImplementedException();
       
    }


    public Actions Click(IWebElement element)
    {
        var e = (PWebElement)element;
        actions.Add(async (proxy) =>
        {
            e.Click();
            await Task.CompletedTask;
            return true;
        });
        return this;
    }

    public Actions Click()
    {
        actions.Add(async (proxy) =>
        {
            var x = proxy.current; if (x == null) return false;
            ElementHandleClickOptions options = new() { Force = true };
            await x.ClickAsync(options);
            return true;
        });
        return this;
    }

    public Actions DoubleClick(IWebElement onElement)
    {
        actions.Add(async (proxy) =>
        {
            var x = (PWebElement)onElement;
            await x.h.ClickAsync(new ElementHandleClickOptions { ClickCount = 2 });
            return true;
        });
        return this;
    }
    public Actions DoubleClick()
    {
        actions.Add(async (proxy) =>
        {
            var x = proxy.current; if (x == null) return false;
            await x.ClickAsync(new ElementHandleClickOptions { ClickCount = 2 });
            return true;
        });
        return this;
    }


    public Actions MoveToElement(IWebElement toElement)
    {
        var e = ((PWebElement)toElement).h;
        actions.Add(async (proxy) =>
        {
            proxy.current = e;
            var options = new ElementHandleHoverOptions { Force = true };
            await e.HoverAsync(options);
            return true;
        });
        return this;
    }


    public Actions MoveToElement(IWebElement toElement, int offsetX, int offsetY)
    {
        return MoveToElement(toElement);
    }

    public Actions MoveByOffset(int offsetX, int offsetY)
    {
        throw new NotImplementedException();

    }

    public Actions ContextClick(IWebElement onElement)
    {
        throw new NotImplementedException();
    }


    public Actions ContextClick()
    {
        throw new NotImplementedException();
    }


    public Actions DragAndDrop(IWebElement source, IWebElement target)
    {
        actions.Add(async (proxy) =>
        {
            var x = (PWebElement)source;
            var y = (PWebElement)target;
            var box = await y.h.BoundingBoxAsync();
            await x.h.HoverAsync();
            await proxy.page.Mouse.DownAsync();
            await proxy.page.Mouse.MoveAsync(box!.X+box.Width/2, box.Y+box.Height/2);
            await y.h.HoverAsync();
            await proxy.page.Mouse.UpAsync();
            return true;
        });
        return this;
    }

    public Actions DragAndDropToOffset(IWebElement source, int offsetX, int offsetY)
    {
        throw new NotImplementedException();
     
    }


    public Actions ScrollToElement(IWebElement element)
    {
        actions.Add(async (proxy) =>
        {
            var x = (PWebElement)element;
            await x.h.ScrollIntoViewIfNeededAsync();
            return true;
        });
        return this;
    }

    public Actions ScrollByAmount(int deltaX, int deltaY)
    {
        ;
        throw new NotImplementedException();
   
    }

    public Actions ScrollFromOrigin(ScrollOrigin scrollOrigin, int deltaX, int deltaY)
    {
        throw new NotImplementedException();

       
    }
    public Actions Pause(TimeSpan duration)
    {
        throw new NotImplementedException();
      
    }
}


public abstract class InputDevice
{
    private string deviceName;


    protected InputDevice(string deviceName)
    {
        if (string.IsNullOrEmpty(deviceName))
        {
            throw new ArgumentException("Device name must not be null or empty", nameof(deviceName));
        }
        this.deviceName = deviceName;
    }


    public string DeviceName
    {
        get { return this.deviceName; }
    }


    public abstract InputDeviceKind DeviceKind { get; }


    public Interaction CreatePause()
    {
        return this.CreatePause(TimeSpan.Zero);
    }

    public Interaction CreatePause(TimeSpan duration)
    {
        return new PauseInteraction(this, duration);
    }

    public override int GetHashCode()
    {
        return this.deviceName.GetHashCode();
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "{0} input device [name: {1}]", this.DeviceKind, this.deviceName);
    }
}

public abstract class Interaction
{
    private InputDevice sourceDevice;
    protected Interaction(InputDevice sourceDevice)
    {
        if (sourceDevice == null)
        {
            throw new ArgumentNullException(nameof(sourceDevice), "Source device cannot be null");
        }

        this.sourceDevice = sourceDevice;
    }

    public InputDevice SourceDevice
    {
        get { return this.sourceDevice; }
    }

    public virtual bool IsValidFor(InputDeviceKind sourceDeviceKind)
    {
        return this.sourceDevice.DeviceKind == sourceDeviceKind;
    }
}

public class KeyInputDevice : InputDevice
{
    public KeyInputDevice()
        : this(Guid.NewGuid().ToString())
    {
    }

    public KeyInputDevice(string deviceName)
        : base(deviceName)
    {
    }

    public override InputDeviceKind DeviceKind
    {
        get { return InputDeviceKind.Key; }
    }

    public Interaction CreateKeyDown(char codePoint)
    {
        return new KeyDownInteraction(this, codePoint);
    }

    public Interaction CreateKeyUp(char codePoint)
    {
        return new KeyUpInteraction(this, codePoint);
    }

    private class KeyDownInteraction : TypingInteraction
    {
        public KeyDownInteraction(InputDevice sourceDevice, char codePoint)
            : base(sourceDevice, "keyDown", codePoint)
        {
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Key down [key: {0}]", Keys.GetDescription(this.Value));
        }
    }

    private class KeyUpInteraction : TypingInteraction
    {
        public KeyUpInteraction(InputDevice sourceDevice, char codePoint)
            : base(sourceDevice, "keyUp", codePoint)
        {
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Key up [key: {0}]", Keys.GetDescription(this.Value));
        }
    }

    private class TypingInteraction : Interaction
    {
        private string type;
        private string value;

        public TypingInteraction(InputDevice sourceDevice, string type, char codePoint)
            : base(sourceDevice)
        {
            this.type = type;
            this.value = codePoint.ToString();
        }

        protected string Value
        {
            get { return this.value; }
        }

    }
}

class PauseInteraction : Interaction
{
    private TimeSpan duration = TimeSpan.Zero;

    public PauseInteraction(InputDevice sourceDevice)
        : this(sourceDevice, TimeSpan.Zero)
    {
    }

    public PauseInteraction(InputDevice sourceDevice, TimeSpan duration)
        : base(sourceDevice)
    {
        this.duration = duration;
    }


    public override bool IsValidFor(InputDeviceKind sourceDeviceKind)
    {
        return true;
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.InvariantCulture, "Pause [duration: {0} ms]", this.duration.TotalMilliseconds);
    }
}



public class PointerInputDevice : InputDevice
{
    private PointerKind pointerKind;

    public PointerInputDevice()
        : this(PointerKind.Mouse)
    {
    }


    public PointerInputDevice(PointerKind pointerKind)
        : this(pointerKind, Guid.NewGuid().ToString())
    {
    }


    public PointerInputDevice(PointerKind pointerKind, string deviceName)
        : base(deviceName)
    {
        this.pointerKind = pointerKind;
    }

    public override InputDeviceKind DeviceKind
    {
        get { return InputDeviceKind.Pointer; }
    }


    public Interaction CreatePointerDown(MouseButton button)
    {
        return CreatePointerDown(button, new PointerEventProperties());
    }
    public Interaction CreatePointerDown(MouseButton button, PointerEventProperties properties)
    {
        return new PointerDownInteraction(this, button, properties);
    }

    public Interaction CreatePointerUp(MouseButton button)
    {
        return CreatePointerUp(button, new PointerEventProperties());
    }
    public Interaction CreatePointerUp(MouseButton button, PointerEventProperties properties)
    {
        return new PointerUpInteraction(this, button, properties);
    }

    public Interaction CreatePointerMove(IWebElement target, int xOffset, int yOffset, TimeSpan duration)
    {
        return CreatePointerMove(target, xOffset, yOffset, duration, new PointerEventProperties());
    }

    public Interaction CreatePointerMove(IWebElement target, int xOffset, int yOffset, TimeSpan duration, PointerEventProperties properties)
    {
        return new PointerMoveInteraction(this, target, CoordinateOrigin.Element, xOffset, yOffset, duration, properties);
    }


    public Interaction CreatePointerMove(CoordinateOrigin origin, int xOffset, int yOffset, TimeSpan duration)
    {
        return CreatePointerMove(origin, xOffset, yOffset, duration, new PointerEventProperties());
    }

    public Interaction CreatePointerMove(CoordinateOrigin origin, int xOffset, int yOffset, TimeSpan duration, PointerEventProperties properties)
    {
        if (origin == CoordinateOrigin.Element)
        {
            throw new ArgumentException("Using a value of CoordinateOrigin.Element without an element is not supported.", nameof(origin));
        }

        return new PointerMoveInteraction(this, null!, origin, xOffset, yOffset, duration, properties);
    }


    public Interaction CreatePointerCancel()
    {
        return new PointerCancelInteraction(this);
    }
}

public class PointerEventProperties
{
    private double? width;
    private double? height;
    private double? pressure;
    private double? tangentialPressure;
    private int? tiltX;
    private int? tiltY;
    private int? twist;
    private double? altitudeAngle;
    private double? azimuthAngle;


    public double? Width
    {
        get { return this.width; }
        set { this.width = value; }
    }


    public double? Height
    {
        get { return this.height; }
        set { this.height = value; }
    }

    public double? Pressure
    {
        get { return this.pressure; }
        set { this.pressure = value; }
    }

    public double? TangentialPressure
    {
        get { return this.tangentialPressure; }
        set { this.tangentialPressure = value; }
    }
    public int? TiltX
    {
        get { return this.tiltX; }
        set { this.tiltX = value; }
    }

    public int? TiltY
    {
        get { return this.tiltY; }
        set { this.tiltY = value; }
    }

    public int? Twist
    {
        get { return this.twist; }
        set { this.twist = value; }
    }

    public double? AltitudeAngle
    {
        get { return this.altitudeAngle; }
        set { this.altitudeAngle = value; }
    }

    public double? AzimuthAngle
    {
        get { return this.azimuthAngle; }
        set { this.azimuthAngle = value; }
    }
}

class PointerDownInteraction : Interaction
{
    private MouseButton button;
    private PointerEventProperties eventProperties;

    public PointerDownInteraction(InputDevice sourceDevice, MouseButton button, PointerEventProperties properties)
        : base(sourceDevice)
    {
        this.button = button;
        this.eventProperties = properties;
    }

    public override string ToString()
    {
        return "Pointer down";
    }
}

class PointerUpInteraction : Interaction
{
    private MouseButton button;
    private PointerEventProperties eventProperties;

    public PointerUpInteraction(InputDevice sourceDevice, MouseButton button, PointerEventProperties properties)
        : base(sourceDevice)
    {
        this.button = button;
        this.eventProperties = properties;
    }

    public override string ToString()
    {
        return "Pointer up";
    }
}

class PointerCancelInteraction : Interaction
{
    public PointerCancelInteraction(InputDevice sourceDevice)
        : base(sourceDevice)
    {
    }

    public override string ToString()
    {
        return "Pointer cancel";
    }
}
class PointerMoveInteraction : Interaction
{
    private IWebElement? target;
    private int x = 0;
    private int y = 0;
    private TimeSpan duration = TimeSpan.MinValue;
    private CoordinateOrigin origin = CoordinateOrigin.Pointer;
    private PointerEventProperties eventProperties;

    public PointerMoveInteraction(InputDevice sourceDevice, IWebElement target, CoordinateOrigin origin, int x, int y, TimeSpan duration, PointerEventProperties properties)
        : base(sourceDevice)
    {
        if (target != null)
        {
            this.target = target;
            this.origin = CoordinateOrigin.Element;
        }
        else
        {
            if (this.origin != CoordinateOrigin.Element)
            {
                this.origin = origin;
            }
        }

        if (duration != TimeSpan.MinValue)
        {
            this.duration = duration;
        }

        this.x = x;
        this.y = y;
        this.eventProperties = properties;
    }

    public override string ToString()
    {
        string originDescription = this.origin.ToString();
        if (this.origin == CoordinateOrigin.Element)
        {
            originDescription = this.target?.ToString()??"";
        }

        return string.Format(CultureInfo.InvariantCulture, "Pointer move [origin: {0}, x offset: {1}, y offset: {2}, duration: {3}ms]", originDescription, this.x, this.y, this.duration.TotalMilliseconds);
    }


}

public class WheelInputDevice : InputDevice
{

    public WheelInputDevice()
        : this(Guid.NewGuid().ToString())
    {
    }

    public WheelInputDevice(string deviceName)
        : base(deviceName)
    {
    }

    public override InputDeviceKind DeviceKind
    {
        get { return InputDeviceKind.Wheel; }
    }


    public Interaction CreateWheelScroll(int deltaX, int deltaY, TimeSpan duration)
    {
        return new WheelScrollInteraction(this, null!, CoordinateOrigin.Viewport, 0, 0, deltaX, deltaY, duration);
    }

    public Interaction CreateWheelScroll(IWebElement target, int xOffset, int yOffset, int deltaX, int deltaY, TimeSpan duration)
    {
        return new WheelScrollInteraction(this, target, CoordinateOrigin.Element, xOffset, yOffset, deltaX, deltaY, duration);
    }

    public Interaction CreateWheelScroll(CoordinateOrigin origin, int xOffset, int yOffset, int deltaX, int deltaY, TimeSpan duration)
    {
        return new WheelScrollInteraction(this, null!, origin, xOffset, yOffset, deltaX, deltaY, duration);
    }
}

public class ScrollOrigin
{
    private IWebElement? element;
    private bool viewport;
    private int xOffset = 0;
    private int yOffset = 0;

    public IWebElement? Element
    {
        get { return this.element; }
        set { this.element = value; }
    }

    public bool Viewport
    {
        get { return this.viewport; }
        set { this.viewport = value; }
    }

    public int XOffset
    {
        get { return this.xOffset; }
        set { this.xOffset = value; }
    }

    public int YOffset
    {
        get { return this.yOffset; }
        set { this.yOffset = value; }
    }

}

class WheelScrollInteraction : Interaction
{
    private IWebElement? target;
    private int x = 0;
    private int y = 0;
    private int deltaX = 0;
    private int deltaY = 0;
    private TimeSpan duration = TimeSpan.MinValue;
    private CoordinateOrigin origin = CoordinateOrigin.Viewport;

    public WheelScrollInteraction(InputDevice sourceDevice, IWebElement target, CoordinateOrigin origin, int x, int y, int deltaX, int deltaY, TimeSpan duration)
        : base(sourceDevice)
    {
        if (target != null)
        {
            this.target = target;
            this.origin = CoordinateOrigin.Element;
        }
        else
        {
            if (this.origin != CoordinateOrigin.Element)
            {
                this.origin = origin;
            }
        }

        if (duration != TimeSpan.MinValue)
        {
            this.duration = duration;
        }

        this.x = x;
        this.y = y;
        this.deltaX = deltaX;
        this.deltaY = deltaY;
    }
}



