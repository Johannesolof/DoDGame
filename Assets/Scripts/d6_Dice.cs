using UnityEngine;
using System.Collections.Generic;

public class d4_Dice : BaseDice 
{
	protected override List<Normal> GetNormals(int sides)
	{
		return new List<Normal>
		{
			new Normal {Value = 1, Direction = new Vector3(-0.5f, -0.3f, 0.8f)},
			new Normal {Value = 2, Direction = new Vector3(-0.5f, -0.3f, -0.8f)},
			new Normal {Value = 3, Direction = new Vector3(0.9f, -0.3f, 0f)},
			new Normal {Value = 4, Direction = new Vector3(0f, 1f, 0f)},
		};
	}
}

public class d6_Dice : BaseDice 
{
    protected override List<Normal> GetNormals(int sides)
    {
        return new List<Normal>
        {
            new Normal {Value = 1, Direction = new Vector3(0, 1, 0)},
            new Normal {Value = 2, Direction = new Vector3(0, 0, -1)},
            new Normal {Value = 3, Direction = new Vector3(1, 0, 0)},
            new Normal {Value = 4, Direction = new Vector3(-1, 0, 0)},
            new Normal {Value = 5, Direction = new Vector3(0, 0, 1)},
            new Normal {Value = 6, Direction = new Vector3(0, -1, 0)},
        };
    }
}

public class d8_Dice : BaseDice
{
    protected override List<Normal> GetNormals(int sides)
    {
        return new List<Normal>
        {
            new Normal {Value = 1, Direction = new Vector3(0, 1, 0)},
            new Normal {Value = 2, Direction = new Vector3(-0.8f, 0.6f, -1)},
            new Normal {Value = 3, Direction = new Vector3(1, 0, 0)},
            new Normal {Value = 4, Direction = new Vector3(-1, 0, 0)},
            new Normal {Value = 5, Direction = new Vector3(0, 0, 1)},
            new Normal {Value = 6, Direction = new Vector3(0, -1, 0)},
            new Normal {Value = 7, Direction = new Vector3(0.8f, -0.6f, -1)},
            new Normal {Value = 8, Direction = new Vector3(-0.8f, -0.6f, -1)},
        };
    }
}

public class d10_Dice : BaseDice
{
	protected override List<Normal> GetNormals(int sides)
	{
		return new List<Normal>
		{
			new Normal {Value = 1, Direction = new Vector3(0.45f, 0.65f, 0.61f)},
			new Normal {Value = 2, Direction = new Vector3(-0.72f, -0.65f, 0.23f)},
			new Normal {Value = 3, Direction = new Vector3(0f, 0.65f, 0.76f)},
			new Normal {Value = 4, Direction = new Vector3(0.45f, -0.65f, -0.61f)},
			new Normal {Value = 5, Direction = new Vector3(-0.44f, 0.65f, 0.62f)},
			new Normal {Value = 6, Direction = new Vector3(0f, -0.65f, 0.76f)},
			new Normal {Value = 7, Direction = new Vector3(0.72f, 0.65f, -0.24f)},
			new Normal {Value = 8, Direction = new Vector3(-0.45f, -0.65f, -0.61f)},
			new Normal {Value = 9, Direction = new Vector3(-0.73f, 0.65f, -0.22f)},
			new Normal {Value = 10, Direction = new Vector3(0.72f, -0.65f, 0.24f)},
		};
	}
}

public class d20_Dice : BaseDice
{
	protected override List<Normal> GetNormals(int sides)
	{
		return new List<Normal>
		{
			new Normal {Value = 1, Direction = new Vector3(0f, -1f, 0f)},
			new Normal {Value = 2, Direction = new Vector3(0.58f, 0.75f, 0.33f)},
			new Normal {Value = 3, Direction = new Vector3(-0.36f, -0.33f, -0.87f)},
			new Normal {Value = 4, Direction = new Vector3(-0.36f, 0.33f, 0.87f)},
			new Normal {Value = 5, Direction = new Vector3(0.58f, -0.33f, 0.75f)},
			new Normal {Value = 6, Direction = new Vector3(-0.93f, 0.33f, -0.13f)},
			new Normal {Value = 7, Direction = new Vector3(0.58f, -0.75f, -0.33f)},
			new Normal {Value = 8, Direction = new Vector3(0f, 0.74f, -0.67f)},
			new Normal {Value = 9, Direction = new Vector3(-0.93f, -0.33f, -0.13f)},
			new Normal {Value = 10, Direction = new Vector3(0.58f, 0.33f, -0.75f)},
			new Normal {Value = 11, Direction = new Vector3(-0.58f, -0.33f, 0.75f)},
			new Normal {Value = 12, Direction = new Vector3(0.93f, 0.33f, -0.13f)},
			new Normal {Value = 13, Direction = new Vector3(0f, -0.74f, 0.67f)},
			new Normal {Value = 14, Direction = new Vector3(-0.58f, 0.75f, 0.33f)},
			new Normal {Value = 15, Direction = new Vector3(0.93f, -0.33f, 0.13f)},
			new Normal {Value = 16, Direction = new Vector3(-0.58f, 0.33f, -0.75f)},
			new Normal {Value = 17, Direction = new Vector3(0.36f, -0.33f, -0.87f)},
			new Normal {Value = 18, Direction = new Vector3(0.36f, 0.33f, 0.87f)},
			new Normal {Value = 19, Direction = new Vector3(-0.58f, -0.75f, -0.33f)},
			new Normal {Value = 20, Direction = new Vector3(0f, 1f, 0f)},
		};
	}
}

public class d100_Dice : BaseDice
{
	protected override List<Normal> GetNormals(int sides)
	{
		return new List<Normal>
		{
			new Normal {Value = 1, Direction = new Vector3(0.45f, 0.65f, 0.61f)},
			new Normal {Value = 2, Direction = new Vector3(-0.72f, -0.65f, 0.23f)},
			new Normal {Value = 3, Direction = new Vector3(0f, 0.65f, 0.76f)},
			new Normal {Value = 4, Direction = new Vector3(0.45f, -0.65f, -0.61f)},
			new Normal {Value = 5, Direction = new Vector3(-0.44f, 0.65f, 0.62f)},
			new Normal {Value = 6, Direction = new Vector3(0f, -0.65f, 0.76f)},
			new Normal {Value = 7, Direction = new Vector3(0.72f, 0.65f, -0.24f)},
			new Normal {Value = 8, Direction = new Vector3(-0.45f, -0.65f, -0.61f)},
			new Normal {Value = 9, Direction = new Vector3(-0.73f, 0.65f, -0.22f)},
			new Normal {Value = 10, Direction = new Vector3(0.72f, -0.65f, 0.24f)},
		};
	}
}