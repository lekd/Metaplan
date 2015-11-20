using System;
using System.IO;

namespace TextToBMP
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var test = new StreamReader ("test5.txt");
			var str = test.ReadToEnd ();
			Console.WriteLine ("{0}", str.Length);

			var text2png = new Text2PNG (5.0f);
			var output = text2png.FromString (str, 2000, 2000);
			output.Save ("test3.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
			Console.WriteLine ("Hello World!");
		}
	}
}
