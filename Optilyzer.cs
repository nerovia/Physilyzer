using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SadConsole;
using SadConsole.DrawCalls;
using SadConsole.Host;
using SadConsole.Input;
using SadConsole.Renderers;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Color = SadRogue.Primitives.Color;

namespace Physilyzer.Optilyzer
{
	struct Model
	{
		public readonly double n1;
		public readonly double n2;
		public readonly double θ1;
		public readonly double θ2;
		public readonly double θr;
		public readonly double θk;
		public readonly double θb;
		public readonly double rte;
		public readonly double rtm;

		public bool tir { get => Math.Abs(θ1) > θk; }

		public Model(double n1, double n2, double θ1)
		{
			this.n1 = n1;
			this.n2 = n2;
			this.θ1 = θ1;
			θ2 = Math.Asin(n1 * Math.Sin(θ1) / n2);
			θr = -θ1;
			θk = Math.Asin(n2 / n1);
			θb = Math.Atan(n2 / n1);
			rte = Math.Pow((n1 * Math.Cos(θ1) - n2 * Math.Cos(θ2)) / (n1 * Math.Cos(θ1) + n2 * Math.Cos(θ2)), 2);
			rtm = Math.Pow((n1 * Math.Cos(θ1) - n2 * Math.Cos(θ2)) / (n1 * Math.Cos(θ2) + n2 * Math.Cos(θ1)), 2);
			rte = double.IsNaN(rte) ? 1 : rte;
			rtm = double.IsNaN(rtm) ? 1 : rtm;
		}
	}

	internal class Screen : SadConsole.UI.ControlsConsole
	{
		readonly TextBox input_n1;
		readonly TextBox input_n2;
		readonly TextBox input_θ1;
		readonly TextBox output_θ2;
		readonly TextBox output_θr;
		readonly TextBox output_θk;
		readonly TextBox output_θb;
		readonly TextBox output_rte;
		readonly TextBox output_rtm;
		readonly Label label_tir;

		Model model = new(1, 2, Convert.ToRAD(30));
		RenderTarget2D canvasTarget;
		VectorBatch vectorBatch;

		public Screen(int width, int height) : base(width, height)
		{
			var y = 1;
			input_n1  = CreateField("n1", 0, 1, y++, true);
			input_n2  = CreateField("n2", 0, 1, y++, true);
			input_θ1  = CreateField("\x00e91", 0, 1, y++, true);
			output_θ2 = CreateField("\x00e92", 0, 1, y++);
			output_θr = CreateField("\x00e9r", 0, 1, y++);
			output_θk = CreateField("\x00e9k", 0, 1, y++);
			output_θb = CreateField("\x00e9b", 0, 1, y++);
			output_rte = CreateField("re", 0, 1, y++);
			output_rtm = CreateField("rm", 0, 1, y++);

			input_n1.SetContent(model.n1);
			input_n2.SetContent(model.n2);
			input_θ1.SetContent(Convert.ToDEG(model.θ1));

			input_n1.TextChanged += InputChanged;
			input_n2.TextChanged += InputChanged;
			input_θ1.TextChanged += InputChanged;

			label_tir = new Label("! total internal reflection") { TextColor = Color.Red, Position = new(1, height - 2) };
			Controls.Add(label_tir);

			canvasTarget = new RenderTarget2D(Global.GraphicsDevice, WidthPixels, HeightPixels);
			vectorBatch = new VectorBatch(Global.GraphicsDevice) { Background = Color.Transparent };

			Refresh();
		}

		private void InputChanged(object? sender, EventArgs e)
		{
			var n1 = input_n1.GetContent(double.NaN);
			var n2 = input_n2.GetContent(double.NaN);
			var θ1 = Convert.ToRAD(input_θ1.GetContent(double.NaN));

			model = new Model(n1, n2, θ1);
			
			Refresh();
		}

		public void Refresh()
		{
			output_θ2.SetContent(Convert.ToDEG(model.θ2));
			output_θr.SetContent(Convert.ToDEG(model.θr));
			output_θk.SetContent(Convert.ToDEG(model.θk));
			output_θb.SetContent(Convert.ToDEG(model.θb));
			output_rte.SetContent(model.rte);
			output_rtm.SetContent(model.rtm);
			label_tir.SetVisibility(model.tir);
			DrawCanvas();
		}

		private TextBox CreateField(string title, object value, int x, int y, bool enabled = false)
		{
			const int width = 6;
			var label = new Label(title) { Position = new(x, y) };
			var input = new TextBox(width) { Position = new(x + label.Width + 1, y), Text = value.ToString()!, IsEnabled = enabled };
			Controls.Add(label);
			Controls.Add(input);
			return input;
		}

		private void DrawCanvas()
		{
			vectorBatch.Clear();
			vectorBatch.Add(-0.5f * Vector3.UnitX, Vector3.UnitX, Color.White);
			vectorBatch.Add(-0.5f * Vector3.UnitY, Vector3.UnitY, Color.Yellow);

			var r = MathF.Sqrt((float)model.rte * (float)model.rtm);
			var unit = 0.7f * Vector3.UnitY;
			var c = Color.Transparent;
			
			var v1 = Vector3.Transform(unit, Matrix.CreateRotationZ((float)model.θ1));
			var c1 = Color.HotPink;
			vectorBatch.Add(Vector3.Zero, v1, c1);

			var v2 = Vector3.Transform(-unit, Matrix.CreateRotationZ((float)model.θ2));
			var c2 = Color.Lerp(Color.Lime, c, r);
			vectorBatch.Add(Vector3.Zero, v2, c2);

			var vr = Vector3.Transform(unit, Matrix.CreateRotationZ((float)model.θr));
			var cr = Color.Lerp(c, Color.Lime, r);
			vectorBatch.Add(Vector3.Zero, vr, cr);

			var vk = Vector3.Transform(unit, Matrix.CreateRotationZ((float)model.θk));
			vectorBatch.Add(Vector3.Zero, vk, vk * new Vector3(-1, 1, 0), Color.DimGray);

			vectorBatch.Draw(canvasTarget);
		}

		public override void Render(TimeSpan delta)
		{
			base.Render(delta);
			var call = new DrawCallTexture(canvasTarget, new(0, 0));
			GameHost.Instance.DrawCalls.Enqueue(call);
		}
	}

	static class ControlExtensions
	{
		public static void SetContent(this TextBox textBox, object content)
		{
			textBox.Text = content.ToString() ?? "";
			textBox.IsDirty = true;
		}

		public static T? GetContent<T>(this TextBox textBox, T fallback) where T : IParsable<T>
		{
			return T.TryParse(textBox.Text, null, out T? result) ? result : fallback;
		}

		public static void SetVisibility(this ControlBase controlBase, bool value)
		{
			controlBase.IsVisible = value;
			controlBase.IsDirty = true;
		}

		public static void SetContent(this TextBox textBox, double value, int digits = 2)
		{
			SetContent(textBox, content: double.Round(value, digits));
		}
	}

	static class Convert
	{
		public static double ToDEG(double rad) => rad * 180d / Math.PI;
		public static double ToRAD(double deg) => deg / 180d * Math.PI;
	}

	class VectorBatch(GraphicsDevice graphicsDevice)
	{
		readonly List<VertexPositionColor> vertices = new();
		readonly List<int> lineIndicies = new();
		readonly List<int> triangleIndicies = new();
		readonly Effect basicEffect = new BasicEffect(graphicsDevice) { VertexColorEnabled = true }; 
		int lineCount = 0;
		int triangleCount = 0;

		public Color Background { get; set; }

		public void Clear()
		{
			vertices.Clear();
			lineIndicies.Clear();
			triangleIndicies.Clear();
			lineCount = 0;
			triangleCount = 0;
		}

		public void Add(Vector3 offset, Vector3 direction, Color color)
		{
			var monoColor = color.ToMonoColor();
			lineCount++;
			lineIndicies.Add(vertices.Count);
			vertices.Add(new VertexPositionColor(offset, monoColor));
			lineIndicies.Add(vertices.Count);
			vertices.Add(new VertexPositionColor(offset + direction, monoColor));
		}

		public void Add(Vector3 a, Vector3 b, Vector3 c, Color color)
		{
			var monoColor = color.ToMonoColor();
			triangleCount++;
			triangleIndicies.Add(vertices.Count);
			vertices.Add(new(a, monoColor));
			triangleIndicies.Add(vertices.Count);
			vertices.Add(new(b, monoColor));
			triangleIndicies.Add(vertices.Count);
			vertices.Add(new(c, monoColor));
		}

		public void Draw(RenderTarget2D renderTarget, Effect? effect = null)
		{
			effect = effect ?? basicEffect;
			graphicsDevice.SetRenderTarget(renderTarget);
			graphicsDevice.Clear(Background.ToMonoColor());
			foreach (var pass in effect.CurrentTechnique.Passes)
			{
				pass.Apply();

				var vertexData = vertices.ToArray();

				var triangleData = triangleIndicies.ToArray();
				graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, 0, vertexData.Length, triangleData, 0, triangleCount);

				var lineData = lineIndicies.ToArray();
				graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, vertexData, 0, vertexData.Length, lineData, 0, lineCount);
			}
		}
	}
}
