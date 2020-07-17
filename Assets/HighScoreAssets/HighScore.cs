using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

using RNG = UnityEngine.Random;

struct HighScoreEntry
{
	public int score;
	public char[] name;

	public string fullStr;
	public int dispPos;
}

public class HighScore : MonoBehaviour
{
	// Standardized logging
	private static int globalLogID = 0;
	private int thisLogID;
	private bool moduleSolved;
	private bool moduleStruck;

	public KMBombInfo bombInfo;
	public KMAudio bombAudio;
	public KMBombModule bombModule;

	public TextMesh headerText;
	public TextMesh[] hsTextLines;
	public KMSelectable[] buttons;
	public Animator[] bAnims;
	public Renderer background;

	private HighScoreEntry[] highScores = new HighScoreEntry[5];
	private char[] correctName = "___".ToCharArray();
	private int entryNum = 0;
	private int cursorOn = 0;

	// Easter egg: Add a credit if a coin is inserted into Laundry after solve.
	private bool readyToInsertCoin = false;
	private int numCredits = 0;

	// ----------
	// Animations
	// ----------

	string ColorByHue(char c, int hue)
	{
		hue += 360;
		hue %= 360;

		float hc = (((hue - 1) % 60) + 1) / 60.0f;
		int hp = (int)Math.Round(255 * hc);

		if (hue <= 60)  return String.Format("<color=#FF{0:X2}00>{1}</color>", hp, c);
		if (hue <= 120) return String.Format("<color=#{0:X2}FF00>{1}</color>", 255 - hp, c);
		if (hue <= 180) return String.Format("<color=#00FF{0:X2}>{1}</color>", hp, c);
		if (hue <= 240) return String.Format("<color=#00{0:X2}FF>{1}</color>", 255 - hp, c);
		if (hue <= 300) return String.Format("<color=#{0:X2}00FF>{1}</color>", hp, c);
		/* else... */   return String.Format("<color=#FF00{0:X2}>{1}</color>", 255 - hp, c);
	}

	IEnumerator TextAnim()
	{
		int i = 0, j = 0;
		string[] ordinals = new string[] {"1ST", "2ND", "3RD", "4TH", "5TH"};

		if (highScores[0].score == 0) // Invalid -- Failed to generate
		{
			ordinals = new string[] {"", "", "", "", "0TH"};
			highScores[0].fullStr = " ^ROM_CHECK_FAIL$";
			highScores[1].fullStr = " *>#^!%:@^$&!*%<@";
			highScores[2].fullStr = " ?#:^_ENTER_AAA_$";
			highScores[3].fullStr = " !&%*^_TO_SOLVE_$";
			highScores[4].fullStr = String.Format("{0,4}{1,9:N0}{2,4}", ordinals[4], highScores[4].score, new string(highScores[i].name));
		}
		else for (; i < 5; ++i)
		{
			highScores[i].fullStr = String.Format("{0,4}{1,9:N0}{2,4}", ordinals[i], highScores[i].score, new string(highScores[i].name));
			highScores[i].dispPos = (i * -2) - 1;
		}

		Color cGreen = new Color(0.0f, 1.0f, 0.0f, 1.0f);
		Color cRed = new Color(1.0f, 0.0f, 0.0f, 1.0f);
		Color cWhite = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		Color cOff = new Color(0.0f, 0.0f, 0.0f, 0.0f);

		while (!moduleSolved)
		{
			while (!moduleSolved && !moduleStruck)
			{
				for (i = 0; i < 5; ++i)
				{
					// Rainbow effect designates the score we're trying to enter.
					if (i == entryNum)
					{
						if (++highScores[i].dispPos < 0)
							continue;
						hsTextLines[i].text = "";

						// Only show the score and position from the precalculated string.
						for (j = 0; j < highScores[i].dispPos && j < 14; ++j)
							hsTextLines[i].text += ColorByHue(highScores[i].fullStr[j], (highScores[i].dispPos - j) * 6);

						// Append the work-in-progress name.
						for (; j < highScores[i].dispPos && j < 17; ++j)
							hsTextLines[i].text += ColorByHue(highScores[i].name[j - 14], (highScores[i].dispPos - j) * 6);
					}
					// All the other scores are plain white.
					else
					{
						if (highScores[i].dispPos > 16 || ++highScores[i].dispPos < 0)
							continue;
						hsTextLines[i].text = highScores[i].fullStr.Substring(0, highScores[i].dispPos);
					}
				}
				yield return new WaitForSeconds(0.05f);
			}

			// Strike/solve animation: Flash red/green
			highScores[entryNum].fullStr = String.Format("{0,4}{1,9:N0}{2,4}", ordinals[entryNum], highScores[entryNum].score, new string(highScores[entryNum].name));
			hsTextLines[entryNum].text = highScores[entryNum].fullStr;

			for (i = 0; i < 9; ++i)
			{
				hsTextLines[entryNum].color = moduleStruck ? cRed : cGreen;
				yield return new WaitForSeconds(0.075f);
				hsTextLines[entryNum].color = cOff;
				yield return new WaitForSeconds(0.075f);
			}
			hsTextLines[entryNum].color = cWhite;

			// Animation ending: reset input if struck.
			if (moduleStruck)
			{
				cursorOn = 0;
				highScores[entryNum].name = "A__".ToCharArray();
				moduleStruck = false;
			}
		}

		// Solve animation: remove text.
		string hTx = "  HIGH  SCORES";
		for (i = 0; i < 5; ++i)
			highScores[i].dispPos = (i * -2) - 1;
		while (highScores[4].dispPos != 16)
		{
			for (i = 0; i < 5; ++i)
			{
				if (highScores[i].dispPos > 16 || ++highScores[i].dispPos < 0)
					continue;
				hsTextLines[i].text = String.Format("{0,17}", highScores[i].fullStr.Substring(highScores[i].dispPos));
			}
			if (highScores[0].dispPos <= 14)
				headerText.text = String.Format("\n{0,15}", hTx.Substring(highScores[0].dispPos));
			yield return new WaitForSeconds(0.05f);
		}
		hsTextLines[4].text = "";
		yield return new WaitForSeconds(0.05f);

		readyToInsertCoin = true;
		hsTextLines[1].text = "   INSERT  COIN";
		hsTextLines[4].text = "   CREDIT(S)  0";
	}

	IEnumerator MoveStarBackground()
	{
		Vector2 tOfs = new Vector2(0.0f, 0.0f);
		while (true)
		{
			tOfs.x += 0.002f;
			background.material.mainTextureOffset = tOfs;
			yield return new WaitForSeconds(0.025f);
		}
	}


	// ------------------
	// Answer calculation
	// (or at least, some of it)
	// ------------------

	// Old arcade games are a little stricter than I normally would be, but
	// an old arcade game is what I'm actually trying to mimic.
	string[] __badWords = new string[] {
		"ASS", "CUM", "FUC", "FUK", "FUQ",
		"FUX", "GAY", "HAG", "JIZ", "KKK",
		"KUM", "SEX", "XXX",
		// The bad pile of three letter words too offensive for even me
		"\u0046\u0041\u0047",
		"\u004E\u0049\u0047",
		"\u0052\u0045\u0045"
	};

	bool BadWordPresent(char[] name)
	{
		return (Array.IndexOf(__badWords, new string(name)) != -1);
	}

	bool CalculateAnswer()
	{
		// Randomly choose what score we're entering for.
		entryNum = RNG.Range(0, 5);

		// Random score generation.
		highScores[0].score = RNG.Range(500, 100500) * 10;
		if (highScores[0].score > 999990)
			highScores[0].score = 999990;
		for (int i = 1; i < 5; ++i)
			highScores[i].score = RNG.Range(highScores[i-1].score / 13, highScores[i-1].score / 10) * 10;

		// Random name generation.
		for (int i = 0; i < 5; ++i)
			highScores[i].name = ((i == entryNum) ? "___" : RandomName()).ToCharArray();

		// Calculation start
		AnswerCalculator ac = new AnswerCalculator(bombInfo.GetSerialNumber(), highScores, entryNum);
		try
		{
			correctName = ac.DoCalculation();
		}
		catch (Exception)
		{
			//Debug.LogFormat("[The High Score #{0}] Exception during processing: {1}", thisLogID, e.ToString());
			//Debug.LogFormat("[The High Score #{0}] Log at that point:", thisLogID);
			//foreach (string s in ac.log)
			//	Debug.LogFormat("[The High Score #{0}] {1}", thisLogID, s);

			return false;
		}
		// Calculation end

		if (BadWordPresent(correctName))
			return false;

		foreach (string s in ac.log)
			Debug.LogFormat("[The High Score #{0}] {1}", thisLogID, s);

		highScores[entryNum].name = "A__".ToCharArray();
		return true;
	}

	void FailureToGenerateState()
	{
		Debug.LogFormat("[The High Score #{0}] E_ROM_CHECK_FAIL", thisLogID);
		Debug.LogFormat("[The High Score #{0}] After 64 attempts, a leaderboard couldn't be generated due to an infinite loop or some other unknown issue.", thisLogID);
		Debug.LogFormat("[The High Score #{0}] The module is going to tell you to input AAA. Do so.", thisLogID);
		correctName = "AAA".ToCharArray();

		entryNum = 4;
		for (int i = 0; i < 5; ++i)
		{
			highScores[i].name = "###".ToCharArray();
			highScores[i].score = 0;
		}
		highScores[entryNum].name = "A__".ToCharArray();
	}

	// -------
	// Startup
	// -------

	private string[] __randomNames = new string[] {
		"ACM", "ASH", "BBH", "BEN", "BOB", 
		"CAR", "CLR", "COM", "DOG", "FOX",
		"IND",/*K.S*/ "KIT", "KRT", "NSA",
		"NME", "LOS", "PLY", "PUP", "REX",
		"SKY", "SIG", "SND", "VOS", "ZEF",

		// Update 1
		"PKA", "RDZ",

		// Update 2
		"SBU", "ULT", 
	};

	string RandomName()
	{
		// Return "K.S" with a fixed probability regardless of number of random names present,
		// because it's relevant for rule K.
		if (RNG.Range(0, 100) < 7) // Approx 7% chance
			return "K.S";
		return __randomNames[RNG.Range(0, __randomNames.Length)];
	}

	void ModuleStartup()
	{
		int attempts = 1;
		for (; attempts <= 64; ++attempts)
		{
			if (CalculateAnswer())
				break;
		}
		if (attempts > 64)
			FailureToGenerateState();

		Debug.LogFormat("[The High Score #{0}] --------------------", thisLogID);
		Debug.LogFormat("[The High Score #{0}] The name to enter is {1}.", thisLogID, new string(correctName));

		StartCoroutine(TextAnim());

		SetupInsertCoinEasterEgg();
	}


	// ------------------
	// Module interaction
	// ------------------

	Coroutine holdRoutine;
	IEnumerator HoldRepeat(int button)
	{
		yield return new WaitForSeconds(0.45f);
		while (true)
		{
			ButtonPressed(button, true);
			yield return new WaitForSeconds(0.075f);
		}
	}

	bool ButtonPressed(int button, bool repeat)
	{
		if (!repeat)
		{
			buttons[button].AddInteractionPunch(0.25f);
			bAnims[button].Play("AnimDown", 0, 0);
			bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

			// Work around double-press bug in the game
			if (holdRoutine != null)
				StopCoroutine(holdRoutine);

			// Arrow buttons can repeat if held for long enough.
			if (button <= 1)
				holdRoutine = StartCoroutine(HoldRepeat(button));
		}

		if (moduleSolved || moduleStruck || correctName[0] == '_')
			return false;

		switch (button)
		{
			case 0: // Up button
				highScores[entryNum].name[cursorOn] = 
					(highScores[entryNum].name[cursorOn] == 'Z') ? 'A' : (char)(highScores[entryNum].name[cursorOn] + 1);
				break;
			case 1: // Down button
				highScores[entryNum].name[cursorOn] = 
					(highScores[entryNum].name[cursorOn] == 'A') ? 'Z' : (char)(highScores[entryNum].name[cursorOn] - 1);
				break;
			case 2: // Submit button
				if (++cursorOn <= 2)
					highScores[entryNum].name[cursorOn] = highScores[entryNum].name[cursorOn - 1];
				else if (highScores[entryNum].name.SequenceEqual(correctName))
				{
					Debug.LogFormat("[The High Score #{0}] SOLVE: Submitted '{1}'. Correct.", thisLogID, new string(highScores[entryNum].name));
					moduleSolved = true;
					bombAudio.PlaySoundAtTransform("OK", transform);
					bombModule.HandlePass();
				}
				else if (BadWordPresent(highScores[entryNum].name))
				{
					Debug.LogFormat("[The High Score #{0}] STRIKE: Submitted a word on the bad words filter. Wrong.", thisLogID);
					highScores[entryNum].name = "NO!".ToCharArray();
					moduleStruck = true;
					bombAudio.PlaySoundAtTransform("NO", transform);
					bombModule.HandleStrike();
				}
				else
				{
					Debug.LogFormat("[The High Score #{0}] STRIKE: Submitted '{1}'. Wrong.", thisLogID, new string(highScores[entryNum].name));
					moduleStruck = true;
					bombAudio.PlaySoundAtTransform("NO", transform);
					bombModule.HandleStrike();
				}
				break;
			case 3: // Back button
				if (cursorOn > 0)
					highScores[entryNum].name[cursorOn--] = '_';
				break;
		}
		return false;
	}

	void ButtonReleased(int button)
	{
		if (holdRoutine != null)
			StopCoroutine(holdRoutine);
		bAnims[button].Play("AnimUp", 0, 0);
		bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
	}

	// Laundry Easter Egg
	IEnumerator DelayUntilCoinLands()
	{
		yield return new WaitForSeconds(1.0f);
		hsTextLines[1].text = "   PRESS  START";
		hsTextLines[4].text = String.Format("   CREDIT(S) {0,2}", ++numCredits);
		bombAudio.PlaySoundAtTransform("Coin", transform);
	}

	bool InsertCoin()
	{
		if (readyToInsertCoin)
			StartCoroutine(DelayUntilCoinLands());
		return false;
	}

	void SetupInsertCoinEasterEgg()
	{
		if (transform.parent == null)
			return;

		for (int i = 0; i < transform.parent.childCount; ++i)
		{
			KMBombModule mod = transform.parent.GetChild(i).gameObject.GetComponent<KMBombModule>();
			if (mod == null || mod.ModuleType != "Laundry")
				continue;

			// We're playing with another module's selectable here, so let's be careful with it.
			Transform csBase = mod.transform.Find("Coin slot holder");
			if (csBase == null)
				continue;

			KMSelectable coinSlot = csBase.GetComponentInChildren<KMSelectable>();
			if (coinSlot == null)
				continue;

			//Debug.LogFormat("[The High Score #{0}] Note: Laundry Easter Egg active.", thisLogID);

			coinSlot.OnInteract += InsertCoin;
		}
	}

	void Awake()
	{
		thisLogID = ++globalLogID;

		for (int i = 0; i < hsTextLines.Length; ++i)
			hsTextLines[i].text = "";

		for (int i = 0; i < buttons.Length; ++i)
		{
			int j = i;
			buttons[i].OnInteract += delegate() {
				return ButtonPressed(j, false);
			};
			buttons[i].OnInteractEnded += delegate() {
				ButtonReleased(j);
			};
		}

		bombModule.OnActivate += ModuleStartup;

		StartCoroutine(MoveStarBackground());
	}

	// -----
	// Twitch Plays support
	// -----

	int BestDirection(char current, char wanted)
	{
		int distance = wanted - current;
		if (distance > 13)  return 1; // Hold DOWN/LEFT
		if (distance < -13) return 0; // Hold UP/RIGHT
		if (distance < 0)   return 1; // Hold DOWN/LEFT
		/* else....     */  return 0; // Hold UP/RIGHT
	}

#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"'!{0} submit BOB' to enter and submit BOB.  Exactly three letters must be given.";
#pragma warning restore 414

	public IEnumerator ProcessTwitchCommand(string command)
	{
		Match mt;
		if ((mt = Regex.Match(command, @"^\s*(?:enter|submit|name)\s+([A-Z]{3})\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)).Success)
		{
			char[] givenName = mt.Groups[1].ToString().ToUpper().ToCharArray();

			// Honestly, it wouldn't surprise me if someone tried putting in a command before the bomb started...
			if (correctName[0] == '_')
			{
				yield return "sendtochaterror The bomb isn't even armed yet! Have you no patience?";
				yield break;
			}
			yield return null;

			// This should never happen in TP, but handling it just in case it does (interactive mode, etc.)
			while (cursorOn > 0) // Reset to the first character
			{
				yield return new KMSelectable[] { buttons[3] };
				yield return new WaitForSeconds(0.1f);
			}

			while (cursorOn < 3)
			{
				// Obviously, if we're on the right character already, we don't need to move to it.
				if (highScores[entryNum].name[cursorOn] != givenName[cursorOn])
				{
					KMSelectable press = buttons[BestDirection(highScores[entryNum].name[cursorOn], givenName[cursorOn])];
					yield return press; // Hold
					while (highScores[entryNum].name[cursorOn] != givenName[cursorOn])
						yield return null; // Wait until the right character is present
					yield return press; // Release
					yield return new WaitForSeconds(0.1f);
				}
				yield return new KMSelectable[] { buttons[2] };
				yield return new WaitForSeconds(0.1f);
			}
		}
		yield break;
	}

	void TwitchHandleForcedSolve()
	{
		if (moduleSolved)
			return;

		Debug.LogFormat("[The High Score #{0}] SOLVE: Force solve requested by Twitch Plays.", thisLogID);
		highScores[entryNum].name = correctName;
		moduleSolved = true;
		bombAudio.PlaySoundAtTransform("OK", transform);
		bombModule.HandlePass();
	}
}
