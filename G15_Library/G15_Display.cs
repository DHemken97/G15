using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G15_Library
{
    public interface IG15_Display
    {
        Bitmap DrawScreen();
    }
    public class G15_Display : IG15_Display
    {
        public Bitmap DrawScreen()
        {
            throw new NotImplementedException();
        }
    }
    public class G15_Dummy_Display : IG15_Display
    {
        public Bitmap DrawScreen()
        {
            throw new NotImplementedException();
        }
    }
}
