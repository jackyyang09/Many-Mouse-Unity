using UnityEngine;

// Inspired by blinker on PlayDate https://sdk.play.date/1.9.3/Inside%20Playdate.html#C-graphics.animation.blinker
[System.Serializable]
public class Blinker  {
	public bool playing;
    
	private bool defaultStateIsOn = true;
	public bool isOn;

    // the number of changes the blinker goes through before it’s complete
    public int cycles = 6;
	public int remainingCycles;

    public bool loop;
	public float blinkRate;

	private float onDuration = 0.2f;
	private float offDuration = 0.2f;

	private float timer;

	public void Start () {
        isOn = defaultStateIsOn;
		timer = 0;
		remainingCycles = cycles;
		playing = true;
	}

	public void Update () {
		if(playing) {
            timer += Time.deltaTime;
            if(remainingCycles > 0 && timer > (isOn ? onDuration : offDuration)) {
                timer = 0;
                isOn = !isOn;
                remainingCycles--;
            }

            if(remainingCycles <= 0) {
                if(loop) {
                    Start();
                } else {
                    playing = false;
                }
            }
        }
	}
}