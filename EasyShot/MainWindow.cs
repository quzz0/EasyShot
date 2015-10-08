using System;
using Gtk;
using EasyShot;
using Gdk;
using System.Linq;

public partial class MainWindow: Gtk.Window
{	
	private bool _selecting = false;
	private Pixbuf pixBuf = null;

	private Button screenButton;
	private Button closeButton;
	private Button copyButton;
	Label logLabel;

	private string screenName = "screen_";
	private string screenEx = "png";

	private int buttonWidth = 100;
	private int buttonHeight = 50;

	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		initSettings ();
		initSelector ();
	}

	public void initSettings() {
		this.SetDefaultSize (0, 0);
		this.Opacity = 0.7;

		int wight, height, x, y = 0;
		this.GetPosition (out x, out y);
		this.GetSize (out wight, out height);

		logLabel = new Label ();

		screenButton = new Gtk.Button ();
		screenButton.SetSizeRequest (buttonWidth, buttonHeight);
		screenButton.Label = "Screenshot";
		screenButton.Clicked += TakeScreen;

		closeButton = new Gtk.Button ();
		closeButton.SetSizeRequest (buttonWidth, buttonHeight);
		closeButton.Label = "Close";
		closeButton.Clicked += Close;

		copyButton = new Gtk.Button ();
		copyButton.SetSizeRequest (buttonWidth, buttonHeight);
		copyButton.Label = "Copy";
		copyButton.Sensitive = false;
		copyButton.Clicked += Copy;

		this.screenPanel.Put (logLabel, (wight / 2) - (buttonWidth / 2), (height / 2) - buttonHeight - buttonHeight - 50);
		this.screenPanel.Put (screenButton, (wight / 2) - (buttonWidth / 2), (height / 2) - buttonHeight - buttonHeight);
		this.screenPanel.Put (copyButton, (wight / 2) - (buttonWidth / 2), (height / 2) - buttonHeight + 10);
		this.screenPanel.Put (closeButton, (wight / 2) - (buttonWidth / 2), (height / 2) - buttonHeight + 10 + buttonHeight + 10);
		this.screenPanel.ShowAll();

		this.screenName = screenName + randomString (6) + "." + screenEx;
	}

	public void initSelector() {
		Gdk.Window rW = Gdk.Global.DefaultRootWindow;

		Gtk.Window w = new Gtk.Window (null);
		w.Opacity = 0.0;
		w.KeepAbove = true;

		DrawingArea da = new DrawingArea ();
		w.Add(da);

		da.MotionNotifyEvent += new MotionNotifyEventHandler (ScribbleMotionNotify);
		da.ButtonPressEvent += new ButtonPressEventHandler (ScribbleButtonPress);
		da.ButtonReleaseEvent += new ButtonReleaseEventHandler (ScribbleButtonRelease);

		da.Events |= EventMask.LeaveNotifyMask | EventMask.ButtonReleaseMask | EventMask.ButtonPressMask |
			EventMask.PointerMotionMask | EventMask.PointerMotionHintMask;

		w.Resize (rW.Screen.Width, rW.Screen.Height);
		w.ShowAll ();
	}

	public void TakeScreen(object obj, EventArgs args)
	{
		this.Opacity = 0.0; //hide easyshot window

		int wight, height, x, y = 0;
		this.GetPosition (out x, out y);
		this.GetSize (out wight, out height);

		System.Threading.Thread.Sleep(500); //waiting for the hiding window

		Gdk.Window window = Gdk.Global.DefaultRootWindow;
		if (window!=null)
		{     
			pixBuf = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8,wight, height);   
			pixBuf.GetFromDrawable(window, Gdk.Colormap.System, x, y, 0, 0, wight, height);  
			//pixBuf.ScaleSimple(window.Screen.Width, window.Screen.Height, Gdk.InterpType.Hyper);
			try {
				pixBuf.Save(screenName, screenEx);
			} catch(Exception ex) {
				logLabel.Text = ex.Message;
			}
		}

		this.Opacity = 0.7;

		//can copy
		copyButton.Sensitive = true;
	}

	private void ScribbleMotionNotify (object o, MotionNotifyEventArgs args)
	{
		/*
		if (!_selecting) 
			return;

		int x, y;
		ModifierType state;
		args.Event.Window.GetPointer (out x, out y, out state);

		int wight, height = 0;
		this.GetSize (out wight, out height);

		this.Resize (x, y);

		//redraw buttons in center
		this.screenPanel.Remove (logLabel);
		this.screenPanel.Remove (screenButton);
		this.screenPanel.Remove (copyButton);
		this.screenPanel.Remove (closeButton);

		this.screenPanel.Put (logLabel, (wight / 2) - (buttonWidth / 2), (height / 2) - buttonHeight - buttonHeight - 50);
		this.screenPanel.Put (screenButton, (wight / 2) - (buttonWidth / 2), (height / 2) - buttonHeight - buttonHeight);
		this.screenPanel.Put (copyButton, (wight / 2) - (buttonWidth / 2), (height / 2) - buttonHeight + 10);
		this.screenPanel.Put (closeButton, (wight / 2) - (buttonWidth / 2), (height / 2) - buttonHeight + 10 + buttonHeight + 10);

		//this.screenPanel.ShowAll();
		*/
		args.RetVal = true;
	}

	private void ScribbleButtonPress (object o, ButtonPressEventArgs args)
	{
		EventButton ev = args.Event;

		if (args.Event.Button == 1) {
			this.Move ((int)ev.X - 2, (int)ev.Y + 64); //2, 64 - idk wtf
			//_selecting = true;
		}
		if (args.Event.Button == 2) {
			this.GrabFocus ();
			this.Present ();
		}
		if (args.Event.Button == 3) {
			int wight, height, x, y = 0;
			this.GetPosition (out x, out y);
			this.GetSize (out wight, out height);

			this.Resize ((int)ev.X - x , (int)ev.Y - y + 64);

			//redraw buttons in center
			this.screenPanel.Remove (logLabel);
			this.screenPanel.Remove (screenButton);
			this.screenPanel.Remove (copyButton);
			this.screenPanel.Remove (closeButton);

			this.screenPanel.Put (logLabel, (wight / 2) - (buttonWidth / 2), (height / 2) - buttonHeight - buttonHeight - 50);
			this.screenPanel.Put (screenButton, (wight / 2) - (buttonWidth / 2), (height / 2) - buttonHeight - buttonHeight);
			this.screenPanel.Put (copyButton, (wight / 2) - (buttonWidth / 2), (height / 2) - buttonHeight + 10);
			this.screenPanel.Put (closeButton, (wight / 2) - (buttonWidth / 2), (height / 2) - buttonHeight + 10 + buttonHeight + 10);

			//_selecting = false;
		}

		args.RetVal = true;
	}

	private void ScribbleButtonRelease (object o, ButtonReleaseEventArgs args)
	{
		//this.GrabFocus ();
		//this.Present ();

		//_selecting = false;
		args.RetVal = true;
	}

	public void Copy(object obj, EventArgs args) {
		Gdk.Atom _atom = Gdk.Atom.Intern("CLIPBOARD", false);
		Gtk.Clipboard _clipBoard = Gtk.Clipboard.Get(_atom);

		if (pixBuf != null)
			_clipBoard.Image = pixBuf;
	}

	public void Close(object obj, EventArgs args) {
		Application.Quit ();
	}

	public static string randomString(int length)
	{

		string upChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		string nums = "0123456789";
		string charSet = upChars + upChars.ToLower () + nums;
		var random = new Random();
		return new string(Enumerable.Repeat(charSet, length)
		                  .Select(s => s[random.Next(s.Length)]).ToArray());
	}

	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}
}
