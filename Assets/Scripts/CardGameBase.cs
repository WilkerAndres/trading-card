﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class CardGameBase : MonoBehaviour {
    public string _name = "";
    public override string ToString() {
        return _name;
    }
}
[System.Serializable]
public class AIGameState
{
    public AIGameState ParentState;
    public List<AIGameState> ChildsStatus=new List<AIGameState>();

    public HeroBehaviourScript PlayerHero;
    //public List<CardBehaviourScript> PlayerHandCards;// = new List<CardBehaviourScript>();
    public List<CardBehaviourScript> PlayerTableCards;// = new List<CardBehaviourScript>();

    public HeroBehaviourScript AIHero;
    public List<CardBehaviourScript> AIHandCards;// = new List<CardBehaviourScript>();
    public List<CardBehaviourScript> AITableCards;// = new List<CardBehaviourScript>();

    public int maxMana;
    public int PlayerMana;
    public int AIMana;

    public BoardBehaviourScript.Turn turn;

    public float State_Score = 0;
    float Attackweight = 1;
    float Healthweight = 1;
    float Manaweight = 1;
    float HeroHealthweight = 0.1f;

    #region Constructors 
    public AIGameState(
        //List<CardBehaviourScript> PlayerHand,
        List<CardBehaviourScript> PlayerTable,
        List<CardBehaviourScript> AIHand,
        List<CardBehaviourScript> AITable,
        HeroBehaviourScript _PlayerHero,
        HeroBehaviourScript _AIHero,
        int _MaxMana,
        int _PlayerMana,
        int _AIMana,
        BoardBehaviourScript.Turn _Turn
        )
    {
        //PlayerHandCards = CardListCopier.DeepCopy(PlayerHand);
        PlayerTableCards = CardListCopier.DeepCopy(PlayerTable);
        PlayerHero = _PlayerHero.Clone() as HeroBehaviourScript;

        AIHandCards = CardListCopier.DeepCopy(AIHand);
        AITableCards = CardListCopier.DeepCopy(AITable);
        AIHero = _AIHero.Clone() as HeroBehaviourScript;
        maxMana = _MaxMana;
        PlayerMana = _PlayerMana;
        AIMana = _AIMana;
        turn = _Turn;
        Calculate_State_Score();
    }
    public AIGameState(
        //List<GameObject> PlayerHand,
         List<GameObject> PlayerTable,
         List<GameObject> AIHand,
         List<GameObject> AITable,
         HeroBehaviourScript _PlayerHero,
         HeroBehaviourScript _AIHero,
         int _MaxMana,
         int _PlayerMana,
         int _AIMana,
         BoardBehaviourScript.Turn _Turn
        )
    {
        //List<CardBehaviourScript> _tempPlayerHand = new List<CardBehaviourScript>();
        //foreach (var item in PlayerHand)_tempPlayerHand.Add( item.GetComponent<CardBehaviourScript>());
        //PlayerHandCards = CardListCopier<List<CardBehaviourScript>>.DeepCopy(_tempPlayerHand);

        List<CardBehaviourScript> _tempPlayerTable = new List<CardBehaviourScript>();
        foreach (var item in PlayerTable) _tempPlayerTable.Add(item.GetComponent<CardBehaviourScript>());
        PlayerTableCards = CardListCopier.DeepCopy(_tempPlayerTable);

        PlayerHero = _PlayerHero.Clone() as HeroBehaviourScript;


        List<CardBehaviourScript> _tempAIHand = new List<CardBehaviourScript>();
        foreach (var item in AIHand) _tempAIHand.Add(item.GetComponent<CardBehaviourScript>());
        AIHandCards = CardListCopier.DeepCopy(_tempAIHand);

        List<CardBehaviourScript> _tempAITable = new List<CardBehaviourScript>();
        foreach (var item in AITable) _tempAITable.Add(item.GetComponent<CardBehaviourScript>());
        AITableCards = CardListCopier.DeepCopy(_tempAITable);

        AIHero = _AIHero.Clone() as HeroBehaviourScript;
        maxMana = _MaxMana;
        PlayerMana = _PlayerMana;
        AIMana = _AIMana;
        turn = _Turn;
        Calculate_State_Score();
    }
    #endregion
    //Evaluate State Score
    #region Calculate Score
    float Calculate_State_Score()
    {
        float AI_Table_Score=0;

        foreach (CardBehaviourScript Card in AITableCards)
        {
            AI_Table_Score += Card._Attack * Attackweight + Card.health * Healthweight;
        }

        float Player_Table_Score = 0;
        foreach (CardBehaviourScript Card in PlayerTableCards)
        {
            Player_Table_Score -= Card._Attack * Attackweight + Card.health * Healthweight;
        }

        float AI_Hand_Score = 0;//Depend On Mana
        foreach (CardBehaviourScript Card in AIHandCards)
        {
            AI_Hand_Score += Card.mana * Manaweight;
        }

        float Player_Health_Score = 0;
        if (PlayerHero.health <= 0) Player_Table_Score = int.MaxValue;
        else
        Player_Health_Score -= PlayerHero.health * HeroHealthweight;


        float AI_Health_Score = 0;
        if (AIHero.health <= 0) AI_Health_Score = int.MinValue;
        else
        AI_Health_Score += AIHero.health * HeroHealthweight;

        State_Score = AI_Table_Score+ Player_Table_Score+ AI_Hand_Score+ Player_Health_Score+ AI_Health_Score;
        return State_Score;
    }
    #endregion

    public void GetAllPlacingAction()
    {
        if (turn == BoardBehaviourScript.Turn.AITurn)
        {
            if (AIHandCards.Count == 0)
            {
                //EndTurn Nothing To Play
            }
            else
            {
                //Generate All Possible Placing
                List<List<CardBehaviourScript>> temp= ProducePlacing(AIHandCards,AIMana);
                for (int i = 0; i < temp.Count; i++)
                {
                    AIGameState State = new AIGameState(PlayerTableCards,AIHandCards,AITableCards,PlayerHero,AIHero,maxMana,PlayerMana,AIMana,turn);
                    //if(temp[i].Count>0)
                    for (int j = 0; j < temp[i].Count; j++)
                    {

                        State.PlaceCard(temp[i][j]);
                        
                    }
                    State.Calculate_State_Score();
                    ChildsStatus.Add(State);
                }
            }
        }
        //Debug.Log("DonePlacing");
    }
    public void GetAllAttackingActions()
    {

    }
    //public CardBehaviourScript Find_Best_Move()
    //{

    //}
    public void PlaceCard(CardBehaviourScript temp)
    {
        //
        //Find That Card
        //
        CardBehaviourScript card = AIHandCards.Find(item => item._name == temp._name);
        if (card.team == CardBehaviourScript.Team.AI && AIMana - card.mana >= 0 && AITableCards.Count < 10)
        {
            AIHandCards.Remove(card);
            AITableCards.Add(card);

            card.SetCardStatus(CardBehaviourScript.CardStatus.OnTable);
            if (card.cardtype == CardBehaviourScript.CardType.Magic)///Apply Magic Effect 
            {
                card.canPlay = true;
                if (card.cardeffect == CardBehaviourScript.CardEffect.ToAll)
                {
                    card.AddToAll(card, delegate { /*card.Destroy(card);*/ });
                }
                else if (card.cardeffect == CardBehaviourScript.CardEffect.ToEnemies)
                {
                    card.AddToEnemies(card, PlayerTableCards, delegate { /*card.Destroy(card);*/ });
                }
            }

            AIMana -= card.mana;
        }
    }
    public List<List<CardBehaviourScript>> ProducePlacing(List<CardBehaviourScript> allValues, int maxmana)
    {
        var collection = new List<List<CardBehaviourScript>>();
        for (int counter = 0; counter < (1 << allValues.Count); ++counter)
        {
            List<CardBehaviourScript> combination = new List<CardBehaviourScript>();
            for (int i = 0; i < allValues.Count; ++i)
            {
                if ((counter & (1 << i)) == 0)
                    combination.Add(allValues[i]);
            }

            // do something with combination
            int manatotal = 0;
            foreach (CardBehaviourScript Card in combination)
            {
                manatotal += Card.mana;

            }
            if (manatotal <= maxmana)
                collection.Add(combination);
        }
        return collection;
    }
}
public static class CardListCopier
{
    public static List<CardBehaviourScript> DeepCopy(List<CardBehaviourScript> objectToCopy)
    {
        List<CardBehaviourScript> temp = new List<CardBehaviourScript>();
        foreach (CardBehaviourScript Card in objectToCopy)
        {
            temp.Add(Card.Clone()as CardBehaviourScript);
        }

        return temp;
    }

}

