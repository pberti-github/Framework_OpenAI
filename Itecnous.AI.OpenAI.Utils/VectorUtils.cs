using System;

namespace Itecnous.AI.OpenAI.Utils;

public static class VectorUtils
{
	public static float[] NormalizeL2(float[] vector)
	{
		if (vector == null || vector.Length == 0)
		{
			return Array.Empty<float>();
		}
		double num = 0.0;
		for (int i = 0; i < vector.Length; i++)
		{
			num += (double)vector[i] * (double)vector[i];
		}
		float num2 = (float)Math.Sqrt(num);
		if ((double)num2 < 1E-10)
		{
			return vector;
		}
		float[] array = new float[vector.Length];
		for (int j = 0; j < vector.Length; j++)
		{
			array[j] = vector[j] / num2;
		}
		return array;
	}
}
