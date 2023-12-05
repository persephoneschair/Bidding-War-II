using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InstructionsManager : MonoBehaviour
{
    public TextMeshProUGUI instructionsMesh;
    public Animator instructionsAnim;
    private readonly string[] instructions = new string[4]
    {
        "<color=#21F1D7><font=LogoFont><size=200%><u>Round 1</u></color>\n" +
        "<color=red>Opening Gambits</size></font></color>\n\n" +

        $"You will face <color=yellow>[###] questions</color>. Prior to each question, a category is shown. Based on the category, <color=yellow>bid from one to three points</color> up to a maximum of your current score. Abstention causes a default bid of one. You have eight seconds to place each bid.\n\n" +

        "<color=green>Answer correctly to earn your bid</color>.\n" +
        "<color=#FF8181>Answer incorrectly or abstain and you lose your bid</color>.\n" +
        "You have fifteen seconds to answer each question.\n\n" +

        "If you run out of points, you are eliminated from the game.\n\n" +

        "Good luck.",



        "<color=#21F1D7><font=LogoFont><size=200%><u>Round 2</u></color>\n" +
        "<color=red>Top Gun</size></font></color>\n\n" +

        $"You will face <color=yellow>[###] questions</color>. Gameplay is identical to round one. The maximum bid will increase by one with each question, starting at five.\n\n" +

        "Additionally, <color=yellow>the fastest high bidder to answer correctly is granted passage to round three</color>. If the fastest high bidder has already been granted passage, passage will be granted to the next fastest high bidder. <color=yellow>Higher bids take priority over speed when ordering</color>.\n\n" +

        "If you run out of points or fail to obtain passage to round three by the conclusion of the final question, you are eliminated from the game.\n\n" +

        "Good luck.",



        "<color=#21F1D7><font=LogoFont><size=200%><u>Round 3</u></color>\n" +
        "<color=red>Survival of the Fittest</size></font></color>\n\n" +

        $"You will face up to <color=yellow>[###] questions</color>. Gameplay is identical to round one. The bid range is now unlimited, with a default minimum bid of five. If you have fewer than five points, your bid will default to your total points.\n\n" +

        "Additionally, <color=yellow>at the end of each question, the player or players with the lowest score will be eliminated</color>.\n\n" +

        "Play continues until only two players remain. In the event multiple eliminations on a single question results in fewer than two players remaining, a player or players with the highest total correct count will be brought back for the final.\n\n" +

        "Good luck.",


        "<color=#21F1D7><font=LogoFont><size=200%><u>Round 4</u></color>\n" +
        "<color=red>Domination</size></font></color>\n\n" +

        $"The array shows fifteen lit circles. The player who was ahead at the conclusion of round three has eight, the runner-up has seven.\n\n" +

        $"You will face up to <color=yellow>[###] questions</color>. Gameplay is identical to round one. The bid range is one to three, with a default bid of one.\n\n" +

        "<color=green>Answer correctly to swing the array in your favour</color>.\n" +
        "<color=#FF8181>Answer incorrectly or abstain to swing the array in your opponent's favour</color>.\n" +
        "Both players' bids are factored into the swing, meaning the maximum swing per question is six.\n\n" +

        "The first player to swing the array entirely to their colour wins the game. If no winner has emerged by the conclusion of the final question, the player with the most lit circles wins.\n\n" +

        "Good luck.",
    };

    [Button]
    public void OnShowInstructions()
    {
        instructionsAnim.SetTrigger("toggle");
        instructionsMesh.text = instructions[(int)GameControl.GetGameControl.currentRound].Replace("[###]", Extensions.NumberToWords(QuestionManager.GetRoundQCount()));
    }

    [Button]
    public void OnHideInstructions()
    {
        instructionsAnim.SetTrigger("toggle");
        //HostManager.GetHost.UpdateLeaderboards();
    }
}
