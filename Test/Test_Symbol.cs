using System;

namespace Test
{
    public class Test_Symbol
    {
        public static void Execute()
        {
            int i = 0;
            for (; ; i += 1)
            {
                int v;
                string s = NNanomsg.NN.Symbol(i, out v);
                if (s == null)
                {
                    break;
                }

                Console.WriteLine(s + ": " + v);
            }
        }
    }
}
