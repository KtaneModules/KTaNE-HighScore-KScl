using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

class AnswerCalculator
{
	public List<string> log = new List<string>();

	// What order the characters are in
	private readonly string __iterationOrder = "AE7VOM8TYG0DK4WC2UHNXP5JS6L13QBRI9FZ?";
	private int iterationPosition = 0;

	private HighScoreEntry[] highScores;
	private int entryNum;

	private char[] correctName;


	// ----------------
	// Serial # Related
	// ----------------

	private string serialNo;

	bool SerialContains(string letters)
	{
		return letters.IndexOfAny(serialNo.ToCharArray()) != -1;
	}

	int SumOfSerialNumberDigits()
	{
		return serialNo.Where((c) => c >= '0' && c <= '9').Select((i) => i - '0').Sum();
	}

	int NumberOfSerialNumberDigits()
	{
		return serialNo.Where((c) => c >= '0' && c <= '9').Distinct().Count();
	}

	char LetterInSerial(int num)
	{
		return serialNo.Where((c) => c < '0' || c > '9').ToArray()[num];
	}

	char RepeatLetterInSerial()
	{
		return serialNo.GroupBy(c => c).Where(c => (c.Key < '0' || c.Key > '9') && c.Count() > 1).Select(c => c.Key).FirstOrDefault();
	}


	// -------------
	// Other Helpers
	// -------------

	bool haveJumped = false;

	bool NumberContains(int num, string letters)
	{
		return letters.IndexOfAny(num.ToString().ToCharArray()) != -1;
	}

	int NumberContainsCount(int num, string letters)
	{
		return letters.Intersect(num.ToString()).Count();
	}

	int NumberOfSpecificDigit(int num, char digit)
	{
		return num.ToString().Where(c => c == digit).Count();
	}

	int SumOfDigits(int number)
	{
		return number.ToString().ToCharArray().Select((i) => i - '0').Sum();
	}

	bool PlayerAboveMe(string search)
	{
		for (int i = entryNum - 1; i >= 0; --i)
		{
			if (highScores[i].name.SequenceEqual(search.ToCharArray()))
				return true;
		}
		return false;
	}


	// ----------
	// Conditions
	// ----------

	bool LetterConditionIsTrue(char l)
	{
		switch (l)
		{
			//Player's score is 1st, and player's name at this point has no characters determined
			case 'A': return entryNum == 0 && correctName.SequenceEqual("___".ToCharArray());

			//Serial number contains a vowel (excluding Y)
			case 'E': return SerialContains("AEIOU");

			//No 7s in the serial number or player's score
			case '7': return !SerialContains("7") && !NumberContains(highScores[entryNum].score, "7");

			//Player is in 3rd, 4th, or 5th
			case 'V': return entryNum >= 2;

			//Player's score contains at least two 0s
			case 'O': return NumberOfSpecificDigit(highScores[entryNum].score, '0') >= 2;

			//Sum of serial number digits ≥ 12
			case 'M': return SumOfSerialNumberDigits() >= 12;

			//Serial number contains a letter in "STANLEY"
			case '8': return SerialContains("STANLEY");

			//Player's score contains no odd digits
			case 'T': return !NumberContains(highScores[entryNum].score, "13579");

			//Player is in 3rd
			case 'Y': return entryNum == 2;

			//Player is in 2nd or 4th
			case 'G': return entryNum == 1 || entryNum == 3;

			//Player's score ≤ 50,000
			case '0': return highScores[entryNum].score <= 50000;

			//Sum of digits in player's score is odd
			case 'D': return (SumOfDigits(highScores[entryNum].score) & 1) == 1;

			//Any score above the player's was set by "K.S"
			case 'K': return PlayerAboveMe("K.S");

			//First character of player's name has been determined
			case '4': return correctName[0] != '_';

			//Sum of digits in player's score ≤ 15
			case 'W': return SumOfDigits(highScores[entryNum].score) <= 15;

			//Have not taken a jump at this point
			case 'C': return !haveJumped;

			//Player is in 1st, and player's score ≥ 750,000
			case '2': return entryNum == 0 && highScores[0].score >= 750000;

			//Serial number contains no vowels
			case 'U': return !SerialContains("AEIOU");

			//Player's name at this point has no characters determined
			case 'H': return correctName.SequenceEqual("___".ToCharArray());

			//200,000 ≤ Player's score ≤ 600,000
			case 'N': return highScores[entryNum].score >= 200000 && highScores[entryNum].score <= 600000;

			//Player's name at this point does not contain an X
			case 'X': return correctName[0] != 'X' && correctName[1] != 'X' && correctName[2] != 'X';

			//Taken at least one jump at this point
			case 'P': return haveJumped;

			//Third character of the player's name has been determined
			case '5': return correctName[2] != '_';

			//Sum of serial number digits ≥ sum of digits in player's score
			case 'J': return SumOfSerialNumberDigits() >= SumOfDigits(highScores[entryNum].score);

			//Player is in 5th, and player's score ≥ 600,000
			case 'S': return entryNum == 4 && highScores[4].score >= 600000;

			//Serial number contains a repeated letter
			case '6': return RepeatLetterInSerial() != '\0';

			//This was the starting line, and no jumps have been taken yet
			case 'L': return serialNo[0] == 'L' && !haveJumped;

			//Player is not in 1st, and 1st place score ≥ 800,000
			case '1': return entryNum > 0 && highScores[0].score >= 800000;

			//Player is not in 3rd
			case '3': return entryNum != 2;

			//Serial number contains J, K, Q, X, or Z
			case 'Q': return SerialContains("JKQXZ");

			//Player's score ≤ 200,000
			case 'B': return highScores[entryNum].score <= 200000;

			//Player's score contains a 4 in any position
			case 'R': return NumberContains(highScores[entryNum].score, "4");

			//Player's score contains all digits in the serial number
			case 'I': return NumberContainsCount(highScores[entryNum].score, serialNo) == NumberOfSerialNumberDigits();

			//Player's score = 999,990
			case '9': return highScores[entryNum].score == 999990;

			//Sum of digits in player's score ≥ 24
			case 'F': return SumOfDigits(highScores[entryNum].score) >= 24;

			//Always
			case 'Z': return true;
		}
		throw new System.InvalidOperationException(String.Format("Attempted to use character {0} for condition", l));
	}


	// -------------
	// Modifications
	// -------------
	int lastCharacterSet = -1;

	char PreviousCharacter()
	{
		return (lastCharacterSet != -1) ? correctName[lastCharacterSet] : LetterInSerial(0);
	}

	void SetCharacter(char rule, int which, char toWhat)
	{
		if (toWhat < 'A' || toWhat > 'Z')
			throw new System.InvalidOperationException(String.Format("Tried to set character {0} to {1}", which + 1, toWhat));

		if (correctName[which] == '_')
		{
			log.Add(String.Format("Rule {0} sets character {1} to {2}.", rule, which + 1, toWhat));
			correctName[which] = toWhat;
			lastCharacterSet = which;
		}
		else
			log.Add(String.Format("Rule {0} tries to set character {1} to {2}, but that character is already set.", rule, which + 1, toWhat));
	}

	void SetNextCharacter(char rule, char toWhat)
	{
		if (correctName[0] == '_')
			SetCharacter(rule, 0, toWhat);
		else if (correctName[1] == '_')
			SetCharacter(rule, 1, toWhat);
		else
			SetCharacter(rule, 2, toWhat);
	}

	void SetCharacterOffset(char rule, int which, char toWhat, int offset)
	{
		string logStrAppend = String.Format("({0}, with an offset of {1}.)", toWhat, offset);

		if (toWhat < 'A' || toWhat > 'Z')
			throw new System.InvalidOperationException(String.Format("Tried to set character {0} to {1}", which + 1, toWhat));

		offset += (toWhat - 'A') + 26;
		offset %= 26;
		toWhat = (char)('A' + offset);
		SetCharacter(rule, which, toWhat);

		log[log.Count - 1] = String.Format("{0} {1}", log[log.Count - 1], logStrAppend);
	}

	void SetNextCharacterOffset(char rule, char toWhat, int offset)
	{
		if (correctName[0] == '_')
			SetCharacterOffset(rule, 0, toWhat, offset);
		else if (correctName[1] == '_')
			SetCharacterOffset(rule, 1, toWhat, offset);
		else
			SetCharacterOffset(rule, 2, toWhat, offset);
	}

	void DoJump(char rule, char toWhere)
	{
		iterationPosition = __iterationOrder.IndexOf(toWhere);
		if (iterationPosition == -1)
			throw new System.InvalidOperationException(String.Format("Tried to jump to {0}", toWhere));

		log.Add(String.Format("Rule {0} jumped to index {1}.", rule, toWhere));
		haveJumped = true;
	}


	void LetterModification(char l)
	{
		switch (l)
		{
			//If this is the first time here, _jump_ to the second character of the serial number. Otherwise, _all three characters_ are A.
			case 'A': 
				// The only situation which makes the latter statement possible is a serial number where the second letter is A.
				if (serialNo[1] == 'A')
				{
					SetCharacter(l, 0, 'A');
					SetCharacter(l, 1, 'A');
					SetCharacter(l, 2, 'A');
					return;
				}
				DoJump(l, serialNo[1]);
				return;

			//The _second character_ is E.
			case 'E': SetCharacter(l, 1, 'E'); return;

			//_Jump_ to the second-to-last digit in the player's score.
			case '7': DoJump(l, (char)('0' + ((highScores[entryNum].score / 10) % 10)) ); return;

			//The _third character_ is the previous character, plus the sum of the digits in the player's score.
			case 'V': SetCharacterOffset(l, 2, PreviousCharacter(), SumOfDigits(highScores[entryNum].score)); return;

			//The _next character_ is the previous character, plus three.
			case 'O': SetNextCharacterOffset(l, PreviousCharacter(), 3); return;

			//The _first character_ is W if the sum of all serial number digits is odd, or M otherwise.
			case 'M': SetCharacter(l, 0, "MW"[SumOfSerialNumberDigits() & 1]); return;

			//The _third character_ is B.
			case '8': SetCharacter(l, 2, 'B'); return;

			//The _second character_ is Z, minus the sum of serial number digits. _Jump_ to the second character if you haven't taken a jump at this point.
			case 'T':
				SetCharacterOffset(l, 1, 'Z', SumOfSerialNumberDigits() * -1);
				if (!haveJumped)
					DoJump(l, correctName[1]);
				return;

			//The _first character_ is the previous character, plus the last digit in the serial number.
			case 'Y': SetCharacterOffset(l, 0, PreviousCharacter(), serialNo[5] - '0'); return;

			//The _next character_ is the second letter in the serial number.
			case 'G': SetNextCharacter(l, LetterInSerial(1)); return;

			//The _third character_ is Z.
			case '0': SetCharacter(l, 2, 'Z'); return;

			//The _first character_ is the fourth character in the serial number, plus 13.
			case 'D': SetCharacterOffset(l, 0, serialNo[3], 13); return;

			//The _second and third characters_ are K and S, respectively.
			case 'K': SetCharacter(l, 1, 'K'); SetCharacter(l, 2, 'S'); return;

			//The _third character_ is the same as the first character.
			case '4': SetCharacter(l, 2, correctName[0]); return;

			//The _next character_ is A, plus the sum of all digits in the 1st player's score.
			case 'W': SetNextCharacterOffset(l, 'A', SumOfDigits(highScores[0].score)); return;

			//Jump to the fourth character in the serial number.
			case 'C': DoJump(l, serialNo[3]); return;

			//The _second character_ is the second character of the name that set the 2nd place score. (If it's a period, use D instead.)
			case '2': SetCharacter(l, 1, highScores[1].name[1] == '.' ? 'D' : highScores[1].name[1]); return;

			//Follow the rule for the fifth character in the serial number, regardless of whether its condition is true or not. (Do not jump to it.)
			case 'U': LetterModification(serialNo[4]); return;

			//The _third character_ is the first letter in the serial number.
			case 'H': SetCharacter(l, 2, LetterInSerial(0)); return;

			//_Jump_ to P.
			case 'N': DoJump(l, 'P'); return;

			//The _next character_ is X.
			case 'X': SetNextCharacter(l, 'X'); return;

			//The _next character_ is the previous character, plus the leftmost number in the player's score.
			case 'P': SetNextCharacterOffset(l, PreviousCharacter(), highScores[entryNum].score.ToString()[0] - '0'); return;

			//The _first character_ is the third character, plus the sum of the digits in the player's score. _Jump_ to the the third character.
			case '5': SetCharacterOffset(l, 0, correctName[2], SumOfDigits(highScores[entryNum].score)); DoJump(l, correctName[2]); return;

			//The _second character_ is the previous character, plus the sum of digits in the serial number.
			case 'J': SetCharacterOffset(l, 1, PreviousCharacter(), SumOfSerialNumberDigits()); return;

			//The _third character_ is S.
			case 'S': SetCharacter(l, 2, 'S'); return;

			//The _next character_ is the repeated letter that appears first in the serial number.
			case '6': SetNextCharacter(l, RepeatLetterInSerial()); return;

			//The _first character_ is L. _Jump_ to W.
			case 'L': SetCharacter(l, 0, 'L'); DoJump(l, 'W'); return;

			//The _first character_ is the first character of the name that set the 1st place score.
			case '1': SetCharacter(l, 0, highScores[0].name[0]); return;

			//The _third character_ is the previous character, minus three.
			case '3': SetCharacterOffset(l, 2, PreviousCharacter(), -3); return;

			//The _next character_ is the previous character, plus 13.
			case 'Q': SetNextCharacterOffset(l, PreviousCharacter(), 13); return;

			//The _next character_ is the previous character, minus one. _Jump_ to A.
			case 'B': SetNextCharacterOffset(l, PreviousCharacter(), -1); DoJump(l, 'A'); return;

			//The _first character_ is the fourth character of the serial number.
			case 'R': SetCharacter(l, 0, serialNo[3]); return;

			//_All three characters_ are Y, O, and U respectively.
			case 'I': SetCharacter(l, 0, 'Y'); SetCharacter(l, 1, 'O'); SetCharacter(l, 2, 'U'); return;

			//The _first and third characters_ are A. _Jump_ to V.
			case '9': SetCharacter(l, 0, 'A'); SetCharacter(l, 2, 'A'); DoJump(l, 'V'); return;

			//The _second character_ is the fifth character of the serial number.
			case 'F': SetCharacter(l, 1, serialNo[4]); return;

			//The _next character_ is the previous character, plus one. _Jump_ to A.
			case 'Z': SetNextCharacterOffset(l, PreviousCharacter(), 1); DoJump(l, 'A'); return;
		}
		throw new System.InvalidOperationException(String.Format("Attempted to use character {0} for modification", l));
	}


	// -----------
	// Calculation
	// -----------

	public AnswerCalculator(string serial, HighScoreEntry[] scores, int entrant)
	{
		serialNo = serial;
		highScores = scores;
		entryNum = entrant;
	}

	public char[] DoCalculation()
	{
		int steps = 0;
		char nextLetter;

		correctName = "___".ToCharArray();
		iterationPosition = __iterationOrder.IndexOf(serialNo[0]);
		if (iterationPosition == -1)
			throw new System.InvalidOperationException(String.Format("First serial number character is {0}", serialNo[0]));

		log.Add(String.Format("{0} is 1st with a score of {1:N0}.", entryNum == 0 ? "The defuser" : new string(highScores[0].name), highScores[0].score));
		log.Add(String.Format("{0} is 2nd with a score of {1:N0}.", entryNum == 1 ? "The defuser" : new string(highScores[1].name), highScores[1].score));
		log.Add(String.Format("{0} is 3rd with a score of {1:N0}.", entryNum == 2 ? "The defuser" : new string(highScores[2].name), highScores[2].score));
		log.Add(String.Format("{0} is 4th with a score of {1:N0}.", entryNum == 3 ? "The defuser" : new string(highScores[3].name), highScores[3].score));
		log.Add(String.Format("{0} is 5th with a score of {1:N0}.", entryNum == 4 ? "The defuser" : new string(highScores[4].name), highScores[4].score));
		log.Add("--------------------");
		log.Add(String.Format("The starting index is {0}.", __iterationOrder[iterationPosition]));

		while (correctName[0] == '_' || correctName[1] == '_' || correctName[2] == '_')
		{
			if (++steps > 128)
				throw new System.InvalidOperationException("Infinite loop detected.");

			nextLetter = __iterationOrder[iterationPosition++];

			if (LetterConditionIsTrue(nextLetter))
			{
				log.Add(String.Format("Rule {0} is true, which results in the following:", nextLetter));
				LetterModification(nextLetter);
			}
			//else
			//	log.Add(String.Format("Rule {0}: Returned false.", nextLetter));
		}

		return correctName;
	}
}