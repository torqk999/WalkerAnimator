//using Sandbox.ModAPI;

namespace IngameScript
{
    partial class Program
    {

        struct PageData
        {
            public static readonly PageData Default = new PageData(0, 0, DefaultDressingSize);

            public int CursorIndex;
            public int LineBufferSize;
            public int HeaderSize;

            public PageData(int cursorIndex, int lineBufferSize, int headerSize)
            {
                CursorIndex = cursorIndex;
                LineBufferSize = lineBufferSize;
                HeaderSize = headerSize;
            }
        }

    }
}
