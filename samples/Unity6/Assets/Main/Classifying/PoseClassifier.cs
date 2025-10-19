using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Linq;
using System.Runtime.InteropServices;

using Emgu.TF.Lite;

class PoseClassifier : System.IDisposable
{
    readonly FlatBufferModel _model;
    readonly Interpreter _interpreter;

    readonly Tensor _inputTensor;
    readonly Tensor _outputTensor;

    readonly float[] _outputData;

    internal PoseClassifier(string modelFilePath)
    {
        if (!File.Exists(modelFilePath))
        {
            throw new FileNotFoundException("Model file not found", modelFilePath);
        }

        _model = new FlatBufferModel(modelFilePath);
        _interpreter = new Interpreter(_model);
        _interpreter.AllocateTensors();

        var inputIndex = _interpreter.InputIndices[0];
        var outputIndex = _interpreter.OutputIndices[0];

        _inputTensor = _interpreter.GetTensor(inputIndex);
        _outputTensor = _interpreter.GetTensor(outputIndex);

        _outputData = new float[_outputTensor.ByteSize / sizeof(float)];
    }

    public void Dispose()
    {
        _model.Dispose();
        _interpreter.Dispose();

        _inputTensor.Dispose();
        _outputTensor.Dispose();
    }

    internal int? Classify(IEnumerable<Vector3> jointNormalizedVectors)
    {
        var inputData = jointNormalizedVectors.SelectMany(
            vector => new float[] { vector.X, vector.Y, vector.Z }
            ).ToArray();

        Marshal.Copy(inputData, 0, _inputTensor.DataPointer, inputData.Length);

        var status = _interpreter.Invoke();
        if (status != Status.Ok)
        {
            return null;
        }

        Marshal.Copy(_outputTensor.DataPointer, _outputData, 0, _outputData.Length);

        var maxIndex = 0;
        for (var i = 1; i < _outputData.Length; i++)
        {
            if (_outputData[maxIndex] < _outputData[i])
            {
                maxIndex = i;
            }
        }

        return maxIndex;
    }
}
