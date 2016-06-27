﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class CardBehaviourScript : CardGameBase, System.ICloneable
{

    public string description = "Description";
    public Texture2D image;
    public int health;
    public int _Attack;
    public int mana;

    public TextMesh nameText;
    public TextMesh healthText;
    public TextMesh AttackText;
    public TextMesh manaText;
    public TextMesh DescriptionText;
    public TextMesh DebugText;


    public bool canPlay = false;
    public enum CardStatus { InDeck, InHand, OnTable, Destroyed };
    public CardStatus cardStatus = CardStatus.InDeck;
    public enum CardType { Monster, Magic };
    public CardType cardtype;
    public enum CardEffect { ToAll, ToEnemies, ToSpecific };
    public CardEffect cardeffect;
    public int AddedHealth;
    public int AddedAttack;
    public enum Team { My, AI };
    public Team team = Team.My;

    public Vector3 newPos;

    float distance_to_screen;
    bool Selected = false;
    public delegate void CustomAction();
    void Start()
    {
        distance_to_screen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z - 1;
        DescriptionText.text = description.ToString();

        cardtype = (CardType)Random.Range(0, 2);


        mana = Random.Range(1, 2);

        //Generate Randome Name
        string[] characters = { "a", "b", "c", "d", "e", "f" };
        for (int i = 0; i < 5; i++)
            _name += characters[Random.Range(0, characters.Length)];
        if (cardtype == CardType.Magic)
        {
            health = 0;
            _Attack = 0;
            AddedAttack = Random.Range(1, 8);
            AddedHealth = Random.Range(1, 8);
            cardeffect = (CardEffect)Random.Range(0, 3);
            if (cardeffect == CardEffect.ToSpecific)
            {
                DescriptionText.text = "Add " + AddedAttack + "/" + AddedHealth + "\n" + "   To Any Selected Moster";
            }
            else if (cardeffect == CardEffect.ToAll)
            {
                DescriptionText.text = "Add " + AddedAttack + "/" + AddedHealth + "\n" + "   To ALL";
            }
            else if (cardeffect == CardEffect.ToEnemies)
            {
                DescriptionText.text = "Add " + AddedAttack + "/" + AddedHealth + "\n" + "   To ALL Enemies";
            }

        }
        else
        {
            //Generate Randome Data
            health = Random.Range(1, 8);
            _Attack = Random.Range(1, 8);
        }
    }
    void FixedUpdate()
    {
        if (!Selected)
        {
            transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 5);
            if (cardStatus != CardStatus.InDeck)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0.0f, 180.0f, 0.0f), Time.deltaTime);
            }
        }

        //Update Visuals
        nameText.text = _name.ToString();
        healthText.text = health.ToString();
        AttackText.text = _Attack.ToString();
        manaText.text = mana.ToString();
        DebugText.text = canPlay ? "Ready to attack" : "Nope";
    }
    public void PlaceCard()
    {
        if (BoardBehaviourScript.instance.turn == BoardBehaviourScript.Turn.MyTurn && cardStatus == CardStatus.InHand && team == Team.My)
        {
            //Selected = false;
            BoardBehaviourScript.instance.PlaceCard(this);
        }
    }
    void OnMouseDown()
    {
        if (cardStatus == CardStatus.InHand)
        {
            Selected = true;
        }

        //if (cardtype==CardType.Monster)//MonsterType
        //{
        if (BoardBehaviourScript.instance.turn == BoardBehaviourScript.Turn.MyTurn && cardStatus == CardStatus.OnTable && team == Team.My)
        {
            if (BoardBehaviourScript.instance.currentCard)
            {
                if (BoardBehaviourScript.instance.currentCard.cardtype == CardType.Magic && BoardBehaviourScript.instance.currentCard.cardeffect == CardEffect.ToSpecific && BoardBehaviourScript.instance.turn == BoardBehaviourScript.Turn.MyTurn && cardStatus == CardStatus.OnTable)//Magic VS Card
                {//What Magic Card Will Do To MosterCard
                    BoardBehaviourScript.instance.targetCard = this;
                    print("Target card: " + _Attack + ":" + health);
                    if (BoardBehaviourScript.instance.currentCard.canPlay)
                    {
                        AddToMonster(BoardBehaviourScript.instance.currentCard, BoardBehaviourScript.instance.targetCard, delegate
                        {
                            BoardBehaviourScript.instance.currentCard.Destroy(BoardBehaviourScript.instance.currentCard);
                        });
                    }
                }
                else
                {
                    //clicked on friendly card on table to attack another table card
                    BoardBehaviourScript.instance.currentCard = this;
                    print("Selected card: " + _Attack + ":" + health);
                }
            }
            else
            {
                //clicked on friendly card on table to attack another table card
                BoardBehaviourScript.instance.currentCard = this;
                print("Selected card: " + _Attack + ":" + health);
            }

        }

        if (BoardBehaviourScript.instance.turn == BoardBehaviourScript.Turn.MyTurn && cardStatus == CardStatus.OnTable && team == Team.AI && BoardBehaviourScript.instance.currentCard)//Card VS Card
        {
            //clicked opponent card on table on your turn
            if (BoardBehaviourScript.instance.currentCard != null && canPlay)
            {
                BoardBehaviourScript.instance.targetCard = this;
                print("Target card: " + _Attack + ":" + health);
                if (BoardBehaviourScript.instance.currentCard.canPlay)
                {
                    AttackCard(BoardBehaviourScript.instance.currentCard, BoardBehaviourScript.instance.targetCard, delegate
                    {
                        BoardBehaviourScript.instance.currentCard.canPlay = false;
                    });
                }
                else print("Card cannot attack");
            }
            print("Cannot Attack this Target card: ");
        }
        else if ((BoardBehaviourScript.instance.turn == BoardBehaviourScript.Turn.MyTurn && BoardBehaviourScript.instance.currentHero && cardStatus == CardStatus.OnTable))//Hero VS Card
        {
            if (BoardBehaviourScript.instance.currentHero.CanAttack)
            {
                BoardBehaviourScript.instance.targetCard = this;
                print("Target card: " + _Attack + ":" + health);
                BoardBehaviourScript.instance.currentHero.AttackCard(BoardBehaviourScript.instance.currentHero, BoardBehaviourScript.instance.targetCard, delegate
                {
                    BoardBehaviourScript.instance.currentHero.CanAttack = false;
                });
            }
        }
        //}
        //else   ////Magic Type Card
        //{

        //}

    }
    void OnMouseUp()
    {
        //Debug.Log("On Mouse Up Event");
        Selected = false;
    }
    void OnMouseOver()
    {

        //Debug.Log("On Mouse Over Event");
    }
    void OnMouseEnter()
    {
        //Debug.Log("On Mouse Enter Event");
        //newPos += new Vector3(0,0.5f,0);
    }
    void OnMouseExit()
    {
        //Debug.Log("On Mouse Exit Event");
        //newPos -= new Vector3(0,0.5f, 0);
    }
    void OnMouseDrag()
    {
        //Debug.Log("On Mouse Drag Event");
        GetComponent<Rigidbody>().MovePosition(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen)));
    }
    public void SetCardStatus(CardStatus status)
    {
        cardStatus = status;
    }
    public void AttackCard(CardBehaviourScript attacker, CardBehaviourScript target, CustomAction action)
    {
        if (attacker.canPlay)
        {
            target.health -= attacker._Attack;
            attacker.health -= target._Attack;

            if (target.health <= 0)
            {
                Destroy(target);
            }

            if (attacker.health <= 0)
            {
                attacker.Destroy(attacker);
            }

            action();
            BoardBehaviourScript.instance.AddHistory(attacker, target);
        }
    }//Attack
    public void AttackHero(CardBehaviourScript attacker, HeroBehaviourScript target, CustomAction action)
    {
        if (attacker.canPlay)
        {
            target.health -= attacker._Attack;
            attacker.health -= target._Attack;

            action();
            BoardBehaviourScript.instance.AddHistory(attacker, target);
        }
    }//Attack
    public void Destroy(CardBehaviourScript card)
    {
        if (card.team == CardBehaviourScript.Team.My)
            BoardBehaviourScript.instance.MyTableCards.Remove(card.gameObject);
        else if (card.team == CardBehaviourScript.Team.AI)
            BoardBehaviourScript.instance.AITableCards.Remove(card.gameObject);


        //BoardBehaviourScript.instance.PlaySound(BoardBehaviourScript.instance.cardDestroy);
        Destroy(card.gameObject);

        BoardBehaviourScript.instance.TablePositionUpdate();
    }
    public void AddToHero(CardBehaviourScript magic, HeroBehaviourScript target, CustomAction action)
    {
        if (magic.canPlay)
        {
            target._Attack += magic.AddedAttack;
            target.health += magic.AddedHealth;

            action();
            BoardBehaviourScript.instance.AddHistory(magic, target);
        }
    }//Magic
    public void AddToMonster(CardBehaviourScript magic, CardBehaviourScript target, CustomAction action)
    {
        if (magic.canPlay)
        {
            target._Attack += magic.AddedAttack;
            target.health += magic.AddedHealth;

            action();
            BoardBehaviourScript.instance.AddHistory(magic, target);
        }
    }//Magic
    public void AddToAll(CardBehaviourScript magic, CustomAction action)
    {
        if (magic.canPlay)
        {
            foreach (var target in BoardBehaviourScript.instance.AITableCards)
            {
                AddToMonster(magic, target.GetComponent<CardBehaviourScript>(), delegate { });
            }
            foreach (var target in BoardBehaviourScript.instance.MyTableCards)
            {
                AddToMonster(magic, target.GetComponent<CardBehaviourScript>(), delegate { });
            }
            action();
        }
    }//Magic
    public void AddToEnemies(CardBehaviourScript magic, List<GameObject> targets, CustomAction action)
    {
        if (magic.canPlay)
        {
            foreach (var target in targets)
            {
                AddToMonster(magic, target.GetComponent<CardBehaviourScript>(), delegate { });
            }
            action();
        }
    }//Magic
    public void AddToEnemies(CardBehaviourScript magic, List<CardBehaviourScript> targets, CustomAction action)
    {
        if (magic.canPlay)
        {
            foreach (var target in targets)
            {
                AddToMonster(magic, target, delegate { });
            }
            action();
        }
    }//Magic

    public object Clone()
    {
        CardBehaviourScript temp = new CardBehaviourScript();
        temp._name = _name;
        temp.description = this.description;
        temp.health = this.health;
        temp._Attack = this._Attack;
        temp.mana = this.mana;
        temp.canPlay = this.canPlay;
        temp.cardStatus = this.cardStatus;
        temp.cardtype = this.cardtype;
        temp.cardeffect = this.cardeffect;
        temp.AddedHealth = this.AddedHealth;
        temp.AddedAttack = this.AddedAttack;
        temp.team = this.team;
        temp.newPos = this.newPos;
        temp.distance_to_screen = this.distance_to_screen;
        temp.Selected = this.Selected;
        return temp;
    }
}
