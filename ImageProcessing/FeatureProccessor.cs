using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing
{
    class JSONManager
    {
        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

        public void Write(ImageFeature[] data)
        {
            List<DataSave> d = new List<DataSave>();
            foreach(ImageFeature f in data)
            {
                DataSave nd = new DataSave();
                nd.name = f.name;
                nd.matrix0 = f.GetMatrix()[0];
                nd.matrix45 = f.GetMatrix()[1];
                nd.matrix90 = f.GetMatrix()[2];
                nd.matrix135 = f.GetMatrix()[3];
                d.Add(nd);
            }
            string jsonString = JsonConvert.SerializeObject(d.ToArray());
            File.WriteAllText("..\\references.json", jsonString);
        }

        public DataSave[] Read()
        {
            string jsonText = File.ReadAllText("..\\references.json");
            return JsonConvert.DeserializeObject<DataSave[]>(jsonText);
        }
    }

    class FeatureProccessor
    {
        Dictionary<string, ImageFeature> baseReference = new Dictionary<string, ImageFeature>();
        JSONManager jsonMgr = new JSONManager();

        public FeatureProccessor()
        {
            LoadReferences();
        }

        public void LoadReferences()
        {
            DataSave[] savedData = jsonMgr.Read();
            baseReference.Clear();
            if (savedData != null)
            {
                foreach (DataSave i in savedData)
                {
                    ImageFeature f = new ImageFeature(i.matrix0, i.matrix45, i.matrix90, i.matrix135);
                    f.name = i.name;
                    AddReference(i.name, f);
                }
            }
        }

        public void AddReference(string referenceName, ImageFeature reference)
        {
            reference.name = referenceName;
            baseReference[referenceName] = reference;
        }

        public FeatureInfo CalculateMatrix(int[,] matrix0, int[,] matrix45, int[,] matrix90, int[,] matrix135)
        {
            ImageFeature inputFeature = new ImageFeature(matrix0, matrix45, matrix90, matrix135);
            string resName = string.Empty;
            float[] avgsre = new float[4];
            float[] avgglu = new float[4];
            float[] avglre = new float[4];
            float[] avgrlu = new float[4];
            float[] avgrpc = new float[4];
            double lastEuclideanFactor = double.MaxValue;

            avgsre = inputFeature.SRE();
            avgglu = inputFeature.GLU();
            avglre = inputFeature.LRE();
            avgrlu = inputFeature.RLU();
            avgrpc = inputFeature.RPC();

            foreach(string s in baseReference.Keys)
            {
                double tempFactor = double.MaxValue;
                float[] tgtValues = baseReference[s].Values();
                float[] tstValues = inputFeature.Values();
                float[] tempValues = new float[20];
                for(int i = 0; i < 20; i++)
                {
                    tempValues[i] = tstValues[i] - tgtValues[i];
                }

                tempFactor = Math.Sqrt(tempValues.Sum());
                Console.WriteLine(tempFactor);
                if(tempFactor < lastEuclideanFactor)
                {
                    resName = s.Split('_')[0];
                    lastEuclideanFactor = tempFactor;
                }
            }

            FeatureInfo result = new FeatureInfo(resName, avgsre, avgglu, avglre, avgrlu, avgrpc);

            return result;
        }

        //public float GetSRE(int[,] matrix)
        //{
        //    float returnVal = 0;
        //    int[] r_j = new int[matrix.GetUpperBound(1)+1];
        //    int[] g_i = new int[matrix.GetUpperBound(0)+1];
        //    float s = 0;

        //    Console.WriteLine(matrix.GetUpperBound(0) + ", " + matrix.GetUpperBound(1));

        //    for(int g = 0; g < g_i.Length; g++)
        //    {
        //        //Console.WriteLine("graylength : " + g_i.Length);
        //        g_i[g] = GetRow(matrix, g).Sum();
        //    }

        //    for(int j = 0; j < r_j.Length; j++)
        //    {
        //        r_j[j] = GetColumn(matrix, j).Sum();
        //    }

        //    s = (float)r_j.Sum();

        //    for (int i = 0; i < r_j.Length; i++)
        //    {
        //        returnVal += ((float)r_j[i] / s) / (float)Math.Pow(i + 1, 2);
        //    }
        //    return returnVal;
        //}

        //IEnumerable<T> GetRow<T>(T[,] array, int row)
        //{
        //    for (int i = 0; i <= array.GetUpperBound(1); ++i)
        //    {
        //        //Console.WriteLine("row : " + row);
        //        //Console.WriteLine("length1 : " + array.GetUpperBound(0)+1);
        //        yield return array[row, i];
        //    }
        //}

        //IEnumerable<T> GetColumn<T>(T[,] array, int column)
        //{
        //    for (int i = 0; i <= array.GetUpperBound(0); ++i)
        //        yield return array[i, column];
        //}
    }

    class FeatureInfo
    {
        private string _result;
        private float[] _sre;
        private float[] _glu;
        private float[] _lre;
        private float[] _rlu;
        private float[] _rpc;

        public FeatureInfo()
        {
            this._result = string.Empty;
        }

        public FeatureInfo(string result, float[] sre, float[] glu, float[] lre, float[] rlu, float[] rpc)
        {
            this._result = result;
            this._sre = sre;
            this._glu = glu;
            this._lre = lre;
            this._rlu = rlu;
            this._rpc = rpc;
        }

        public string Result
        {
            get
            {
                return this._result;
            }
        }

        public float SRE(int angle)
        {
            return this._sre[(int)Math.Floor((decimal)angle / 45)];
        }

        public float GLU(int angle)
        {
            return this._glu[(int)Math.Floor((decimal)angle / 45)];
        }

        public float LRE(int angle)
        {
            return this._lre [(int)Math.Floor((decimal)angle / 45)];
        }

        public float RLU(int angle)
        {
            return this._rlu [(int)Math.Floor((decimal)angle / 45)];
        }

        public float RPC(int angle)
        {
            return this._rpc [(int)Math.Floor((decimal)angle / 45)];
        }
    }

    class ImageFeature
    {
        private string _name;
        private ImageMatrix[] _matrix;
        private int[][,] _rawMatrix;
        private float[] _sre = new float[4];
        private float[] _glu = new float[4];
        private float[] _lre = new float[4];
        private float[] _rlu = new float[4];
        private float[] _rpc = new float[4];

        public ImageFeature(int[,] matrix0, int[,] matrix45, int[,] matrix90, int[,] matrix135)
        {
            _rawMatrix = new int[4][,]
            {
                matrix0, matrix45, matrix90, matrix135
            };
            _matrix = new ImageMatrix[4] {
                new ImageMatrix(matrix0),
                new ImageMatrix(matrix45),
                new ImageMatrix(matrix90),
                new ImageMatrix(matrix135)
            };
            Init();
        }

        private void Init()
        {
            for(int i = 0; i < 4; i++)
            {
                _sre[i] = _matrix[i].SRE;
                _glu[i] = _matrix[i].GLU;
                _lre[i] = _matrix[i].LRE;
                _rlu[i] = _matrix[i].RLU;
                _rpc[i] = _matrix[i].RPC;
            }
        }

        public string name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public int[][,] GetMatrix()
        {
            return this._rawMatrix;
        }

        public float[] Values()
        {
            float[] retVal = new float[20];

            Array.Copy(_sre, retVal, 4);
            Array.Copy(_glu, 0, retVal, 4, 4);
            Array.Copy(_lre, 0, retVal, 8, 4);
            Array.Copy(_rlu, 0, retVal, 12, 4);
            Array.Copy(_rpc, 0, retVal, 16, 4);

            return retVal;
        }

        public float[] SRE()
        {
            return this._sre;
        }

        public float[] GLU()
        {
            return this._glu;
        }

        public float[] LRE()
        {
            return this._lre;
        }

        public float[] RLU()
        {
            return this._rlu;
        }

        public float[] RPC()
        {
            return this._rpc;
        }
    }

    class DataSave
    {
        public string name;
        public int[,] matrix0;
        public int[,] matrix45;
        public int[,] matrix90;
        public int[,] matrix135;
    }
}
