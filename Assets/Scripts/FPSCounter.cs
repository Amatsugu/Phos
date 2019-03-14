using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TMP_Text))]
public class FPSCounter : MonoBehaviour
{
	public float updateInterval = 0.5F;
	public Gradient fpsColor = new Gradient();

	private float accum = 0; // FPS accumulated over the interval
	private int frames = 0; // Frames drawn over the interval
	private float timeleft; // Left time for current interval
	private TMP_Text t;
	void Start()
	{
		timeleft = updateInterval;
		t = GetComponent<TMP_Text>();
	}
	void Update()
	{
		//if (t == null)
			//Start();
		timeleft -= Time.deltaTime;
		accum += Time.timeScale / Time.deltaTime;
		++frames;
		// Interval ended - update GUI text and start new interval
		if (timeleft <= 0.0)
		{
			// display two fractional digits (f2 format)
			float fps = accum / frames;
			t.text  = $"{((int)(fps * 100))/100} FPS";
			t.color = fpsColor.Evaluate((fps - 15) / 60f);
			//DebugConsole.Log(format, level);
			timeleft = updateInterval;
			accum = 0.0F;
			frames = 0;
		}
	}
}