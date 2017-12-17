using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MapEditor
{
    static class Program
    {
        public static Form Form;
        public static Font font = new Font(FontFamily.GenericSansSerif, 10);
        

        [STAThread]
        private static void Main()
        {
            InitializeForm();
            // 1 - graphics, 3 - Размер ячейки, 4 - Ширина линий (четное значение даст доп. эффект обводки)
            Application.Run(Form);
        }

        public static void InitializeForm()
        {
            Form = new Form
            {
                Text = "Map Editor",
                ClientSize = new Size(860, 600),
                BackColor = Color.Black,
                AutoScroll = true
            };

            Form.Resize += (s, e) => Form.Invalidate();
            Form.Paint += (sender, ev) => Paint((Form)sender, ev);
            Form.KeyDown += (sender, ev) => Field.KeyDown((Form)sender, ev);
            Form.MouseDown += (sender, ev) => Field.MouseDown((Form)sender, ev);
            Form.MouseUp += (sender, ev) => Field.MouseUp((Form)sender, ev);
            Form.MouseMove += (sender, ev) => Field.MouseMove((Form)sender, ev);
        }

        private static void Paint(Form form, PaintEventArgs e)
        {
            Field.InitializeField(Form.CreateGraphics(), 30, 20, 4);
            Field.DrawMatrix();
            Field.FillAllRectangles();
        }

        

    }
}
/*


using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Manipulation
{
    public class Program
    {
        public static Form Form;
        [STAThread]
        private static void Main()
        {
            InitializeForm();
            Application.Run(Form);
        }

        public static void InitializeForm()
        {
            Form = new AntiFlickerForm
            {
                Text = "Manipulator",
                ClientSize = new Size(800, 600)
            }; //можете заменить AntiFlickerForm на Form, запустить и подвигать мышкой.

            Form.Paint += (sender, ev) => Paint((Form)sender, ev);
            Form.KeyDown += (sender, ev) => VisualizerTask.KeyDown((Form)sender, ev);
            Form.MouseMove += (sender, ev) => VisualizerTask.MouseMove((Form)sender, ev);
            Form.MouseWheel += (sender, ev) => VisualizerTask.MouseWheel((Form)sender, ev);
            Form.Resize += (s, e) => Form.Invalidate();
        }

        private static void Paint(Form form, PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.FillRectangle(VisualizerTask.UnreachableAreaBrush, 0, 0, form.ClientSize.Width, form.ClientSize.Height);
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            var shoulderPos = new PointF(form.ClientSize.Width / 2f, form.ClientSize.Height / 2f);
            VisualizerTask.DrawManipulator(graphics, shoulderPos);
        }
    }
}
*/