using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing
{
    class ImageMatrix
    {
        int[,] _val;
        int[] r_j;
        int[] g_i;
        float _s;
        float _sre;
        float _lre;
        float _glu;
        float _rlu;
        float _rpc;

        public ImageMatrix(int[,] value)
        {
            this._val = value;
            Init();
        }

        public ImageMatrix()
        {
            this._val = new int[0, 0];
        }

        public void SetValue(int[,] value)
        {
            this._val = value;
            Init();
        }

        private void Init()
        {
            this._sre = 0;
            this._lre = 0;
            this._glu = 0;
            this._rlu = 0;
            this._rpc = 0;
            this.r_j = new int[_val.GetUpperBound(1) + 1];
            this.g_i = new int[_val.GetUpperBound(0) + 1];

            for (int g = 0; g < g_i.Length; g++)
            {
                g_i[g] = GetRow(_val, g).Sum();
            }

            for (int j = 0; j < r_j.Length; j++)
            {
                r_j[j] = GetColumn(_val, j).Sum();
            }

            this._s = r_j.Sum();

            for (int i = 0; i < r_j.Length; i++)
            {
                _sre += ((float)r_j[i] / _s) / (float)Math.Pow(i + 1, 2);
                _lre += ((float)r_j[i] * ((float)Math.Pow(i + 1, 2))) / _s;
                _rlu += (float)Math.Pow(r_j[i], 2) / _s;
                _rpc += (float)r_j[i] / (r_j.Length * g_i.Length);
            }

            for(int i = 0; i < g_i.Length; i++)
            {
                _glu += (float)Math.Pow(g_i[i], 2) / _s;
            }

        }

        public float SRE
        {
            get
            {
                return _sre;
            }
        }

        public float LRE
        {
            get
            {
                return _lre;
            }
        }

        public float RLU
        {
            get
            {
                return _rlu;
            }
        }

        public float GLU
        {
            get
            {
                return _glu;
            }
        }

        public float RPC
        {
            get
            {
                return _rpc;
            }
        }

        IEnumerable<T> GetRow<T>(T[,] array, int row)
        {
            for (int i = 0; i <= array.GetUpperBound(1); ++i)
                yield return array[row, i];
        }

        IEnumerable<T> GetColumn<T>(T[,] array, int column)
        {
            for (int i = 0; i <= array.GetUpperBound(0); ++i)
                yield return array[i, column];
        }
    }
}
