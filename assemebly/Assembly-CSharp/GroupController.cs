﻿using System;
using System.Collections.Generic;
using Pathfinding.RVO;
using Pathfinding.RVO.Sampled;
using UnityEngine;

// Token: 0x0200006B RID: 107
public class GroupController : MonoBehaviour
{
	// Token: 0x060003AC RID: 940 RVA: 0x0001B508 File Offset: 0x00019908
	public void Start()
	{
		this.cam = Camera.main;
		RVOSimulator rvosimulator = UnityEngine.Object.FindObjectOfType(typeof(RVOSimulator)) as RVOSimulator;
		if (rvosimulator == null)
		{
			base.enabled = false;
			throw new Exception("No RVOSimulator in the scene. Please add one");
		}
		this.sim = rvosimulator.GetSimulator();
	}

	// Token: 0x060003AD RID: 941 RVA: 0x0001B560 File Offset: 0x00019960
	public void Update()
	{
		if (Screen.fullScreen && Screen.width != Screen.resolutions[Screen.resolutions.Length - 1].width)
		{
			Screen.SetResolution(Screen.resolutions[Screen.resolutions.Length - 1].width, Screen.resolutions[Screen.resolutions.Length - 1].height, true);
		}
		if (this.adjustCamera)
		{
			List<Agent> agents = this.sim.GetAgents();
			float num = 0f;
			for (int i = 0; i < agents.Count; i++)
			{
				float num2 = Mathf.Max(Mathf.Abs(agents[i].InterpolatedPosition.x), Mathf.Abs(agents[i].InterpolatedPosition.z));
				if (num2 > num)
				{
					num = num2;
				}
			}
			float a = num / Mathf.Tan(this.cam.fieldOfView * 0.0174532924f / 2f);
			float b = num / Mathf.Tan(Mathf.Atan(Mathf.Tan(this.cam.fieldOfView * 0.0174532924f / 2f) * this.cam.aspect));
			this.cam.transform.position = Vector3.Lerp(this.cam.transform.position, new Vector3(0f, Mathf.Max(a, b) * 1.1f, 0f), Time.smoothDeltaTime * 2f);
		}
		if (Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.Mouse0))
		{
			this.Order();
		}
	}

	// Token: 0x060003AE RID: 942 RVA: 0x0001B70C File Offset: 0x00019B0C
	private void OnGUI()
	{
		if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && !Input.GetKey(KeyCode.A))
		{
			this.Select(this.start, this.end);
			this.wasDown = false;
		}
		if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
		{
			this.end = Event.current.mousePosition;
			if (!this.wasDown)
			{
				this.start = this.end;
				this.wasDown = true;
			}
		}
		if (Input.GetKey(KeyCode.A))
		{
			this.wasDown = false;
		}
		if (this.wasDown)
		{
			Rect position = Rect.MinMaxRect(Mathf.Min(this.start.x, this.end.x), Mathf.Min(this.start.y, this.end.y), Mathf.Max(this.start.x, this.end.x), Mathf.Max(this.start.y, this.end.y));
			if (position.width > 4f && position.height > 4f)
			{
				GUI.Box(position, string.Empty, this.selectionBox);
			}
		}
	}

	// Token: 0x060003AF RID: 943 RVA: 0x0001B870 File Offset: 0x00019C70
	public void Order()
	{
		Ray ray = this.cam.ScreenPointToRay(Input.mousePosition);
		RaycastHit raycastHit;
		if (Physics.Raycast(ray, out raycastHit))
		{
			float num = 0f;
			for (int i = 0; i < this.selection.Count; i++)
			{
				num += this.selection[i].GetComponent<RVOController>().radius;
			}
			float num2 = num / 3.14159274f;
			num2 *= 2f;
			for (int j = 0; j < this.selection.Count; j++)
			{
				float num3 = 6.28318548f * (float)j / (float)this.selection.Count;
				Vector3 target = raycastHit.point + new Vector3(Mathf.Cos(num3), 0f, Mathf.Sin(num3)) * num2;
				this.selection[j].SetTarget(target);
				this.selection[j].SetColor(this.GetColor(num3));
				this.selection[j].RecalculatePath();
			}
		}
	}

	// Token: 0x060003B0 RID: 944 RVA: 0x0001B990 File Offset: 0x00019D90
	public void Select(Vector2 _start, Vector2 _end)
	{
		_start.y = (float)Screen.height - _start.y;
		_end.y = (float)Screen.height - _end.y;
		Vector2 b = Vector2.Min(_start, _end);
		Vector2 a = Vector2.Max(_start, _end);
		if ((a - b).sqrMagnitude < 16f)
		{
			return;
		}
		this.selection.Clear();
		RVOExampleAgent[] array = UnityEngine.Object.FindObjectsOfType(typeof(RVOExampleAgent)) as RVOExampleAgent[];
		for (int i = 0; i < array.Length; i++)
		{
			Vector2 vector = this.cam.WorldToScreenPoint(array[i].transform.position);
			if (vector.x > b.x && vector.y > b.y && vector.x < a.x && vector.y < a.y)
			{
				this.selection.Add(array[i]);
			}
		}
	}

	// Token: 0x060003B1 RID: 945 RVA: 0x0001BAA4 File Offset: 0x00019EA4
	public Color GetColor(float angle)
	{
		return GroupController.HSVToRGB(angle * 57.2957764f, 0.8f, 0.6f);
	}

	// Token: 0x060003B2 RID: 946 RVA: 0x0001BABC File Offset: 0x00019EBC
	private static Color HSVToRGB(float h, float s, float v)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = s * v;
		float num5 = h / 60f;
		float num6 = num4 * (1f - Math.Abs(num5 % 2f - 1f));
		if (num5 < 1f)
		{
			num = num4;
			num2 = num6;
		}
		else if (num5 < 2f)
		{
			num = num6;
			num2 = num4;
		}
		else if (num5 < 3f)
		{
			num2 = num4;
			num3 = num6;
		}
		else if (num5 < 4f)
		{
			num2 = num6;
			num3 = num4;
		}
		else if (num5 < 5f)
		{
			num = num6;
			num3 = num4;
		}
		else if (num5 < 6f)
		{
			num = num4;
			num3 = num6;
		}
		float num7 = v - num4;
		num += num7;
		num2 += num7;
		num3 += num7;
		return new Color(num, num2, num3);
	}

	// Token: 0x040002F8 RID: 760
	public GUIStyle selectionBox;

	// Token: 0x040002F9 RID: 761
	public bool adjustCamera = true;

	// Token: 0x040002FA RID: 762
	private Vector2 start;

	// Token: 0x040002FB RID: 763
	private Vector2 end;

	// Token: 0x040002FC RID: 764
	private bool wasDown;

	// Token: 0x040002FD RID: 765
	private List<RVOExampleAgent> selection = new List<RVOExampleAgent>();

	// Token: 0x040002FE RID: 766
	private Simulator sim;

	// Token: 0x040002FF RID: 767
	private Camera cam;

	// Token: 0x04000300 RID: 768
	private const float rad2Deg = 57.2957764f;
}
