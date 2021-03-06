﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.IO;
using TMPro;

public class GameControl : MonoBehaviour
{

	Utilities utils = new Utilities();
	
	//Game Variables
	bool gameStarted = false;
	bool gameOver = false;
	int dealer = 3;
	public bool dealing = false;
	public bool shuffling = false;
	//------------
	
	//Card Variables
	Card[] daCards;
	Card[] actionCards;
	Card[] talentCards;
	public int curActionCardsIdx = 0;
	public int curActionDiscardIdx = 0;
	public int curTalentCardsIdx = 0;
	public int curTalentDiscardIdx = 0;
	const int ActionCardCount = 50;
	const int TalentCardCount = 70;
	const int MaxCardsInHand = 7;
	const int CardDrawnHandIDX = 99;
	//------------
	
	//Player Variables
	public Player[] player;
	public int playerCount;
	public int curPlayer = -1;
	int holdPlayer = -1;
	public int thePlayerIndex;
	//------------

	//Canvas/Hud/Misc Variables 
	public NavCanvas gNavCanvas;
	public GameObject gMovieBackground;
	
	const int MovieButtonIdx = 1;
	
	public MovieTitles movieTitles;	
	
	//------------
	
	// Start is called before the first frame update
	void Start()
    {
	    gNavCanvas = FindObjectOfType<NavCanvas>();
	    InitHud();
	}

    // Update is called once per frame
	void Update()
    {
	    //if(!gameOver)
	    //{
		//    if (gameStarted && shuffling == false)
		//    {
		//	    if (holdPlayer != curPlayer)
		//	    {

		//		    ReshuffleCheck();

		//			if(shuffling == false)
		//		    {
		//			    holdPlayer = curPlayer;
						
		//				DoTickerStartTurnMessage();
						
		//				player[curPlayer].DoTurn();				    	
		//		    }
		//	    }
		//	}	    	
	    //}
	    //else
	    //{
	    //	//Game Over stuff
	    //}
	}

	void InitHud()
	{
		gNavCanvas.InitHub();
	}
	
	public void StartGame()
	{
		gNavCanvas.StartGame();
		StartCoroutine("InitGame");
	}

	public IEnumerator InitGame()
	{
		
		gNavCanvas.SetTicker("Initializing Game...");
		//sets the player number on this machine
		thePlayerIndex = 0;

		//get all the cards
		daCards = FindObjectsOfType<Card>();
		
		//Initialize Main Varaibles
		InitActionCards();
		InitTalentCards();
		InitPlayers();
		LoadMovieTitles();
		

		//wait for 2 seconds. cards should have fallen by then
		yield return new WaitForSeconds(2f);
		
		//stop gravity on all cards
		foreach (Card item in daCards)
		{
			item.GetComponent<Rigidbody>().isKinematic = true;
		}
		
		//move cards in both decks physically to match order in deck
		actionCards = actionCards.OrderBy(go => go.cardData.deckIdx).ToArray();
		float nvalue = 0.02f;
		Vector3 v = actionCards[0].transform.position;
		for (int i = 49; i >= 0; i--)
		{
			actionCards[i].transform.position = new Vector3(v.x, nvalue, v.z);
			nvalue += .005f;
		}
		talentCards = talentCards.OrderBy(go => go.cardData.deckIdx).ToArray();
		nvalue = 0.02f;
		v = talentCards[0].transform.position;
		for (int i = 69; i >= 0; i--)
		{
			talentCards[i].transform.position = new Vector3(v.x, nvalue, v.z);
			nvalue += .005f;
		}
		
		//Deal Cards
		yield return StartCoroutine("DealCards", dealer);
		//Start Game to Started and run main game loop
		gameStarted = true;
		StartCoroutine("MainGameLoop");
	}

	IEnumerator MainGameLoop()
	{
		while(!gameOver)
		{
			if (gameStarted && shuffling == false)
			{
				if (holdPlayer != curPlayer)
				{
					yield return StartCoroutine("ReshuffleCheck");
					if(shuffling == false)
					{
						holdPlayer = curPlayer;
						
						DoTickerStartTurnMessage();
						yield return player[curPlayer].DoTurn();				    	
					}
					yield return null;
				}
				yield return null;
			}
			yield return null;
		}
		if(gameOver)
		{
			//Game Over stuff
			yield return null;
		}
	}

	void InitActionCards()
	{
		//move cards to starting location
		foreach (Card item in daCards)
		{
			if(item.cardData.type == CardData.CardType.Action)
			{
				item.transform.position = new Vector3(-0.9f,2f,0);
				//item.transform.DORotate(new Vector3(180f,180f,0), 0);				
			}
		}	
		//Get Action Cards
		int count = 0;
		actionCards = new Card[ActionCardCount];
		foreach (Card item in daCards)
		{
			if (item.cardData.type == CardData.CardType.Action)
			{
				actionCards[count] = item;
				actionCards[count].cardData.status = CardData.Status.Deck;
				actionCards[count].cardData.hand = 0;
				actionCards[count].cardData.handIdx = -1;
				actionCards[count].cardData.deckIdx = count; //actionCards[count].cardData.cardID;
				actionCards[count].cardData.movie = -1;
				actionCards[count].cardData.movieIdx = -1;
				count += 1;
			}
		}
		//shuffle
		utils.ShuffleCards(actionCards);

		//Sort action cards by shuffle index
		actionCards = actionCards.OrderBy(go => go.cardData.deckIdx).ToArray();
		//move cards to the height to drop from
		float loc = 2f;
		Vector3 v = actionCards[0].transform.position;
		for (int i = 49; i >= 0; i--)
		{
			loc = loc + 0.1f;
			actionCards[i].transform.position = new Vector3(v.x, loc, v.z);
		}
		//turn off Kinematic to activate gravity
		foreach (Card item in actionCards)
		{
			item.GetComponent<Rigidbody>().isKinematic = false;
		}

	}
	
	void InitTalentCards()
	{
		//move cards to starting location
		foreach (Card item in daCards)
		{
			if(item.cardData.type == CardData.CardType.Talent)
			{
				item.transform.position = new Vector3(3f,2f,0);
				//item.transform.DORotate(new Vector3(180f,180f,0), 0);
			}
		}	
		//Get Talent Cards
		int count = 0;
		talentCards = new Card[TalentCardCount];
		foreach (Card item in daCards)
		{
			if (item.cardData.type == CardData.CardType.Talent)
			{
				talentCards[count] = item;
				talentCards[count].cardData.status = CardData.Status.Deck;
				talentCards[count].cardData.hand = -1;
				talentCards[count].cardData.handIdx = -1;
				talentCards[count].cardData.deckIdx = count; //talentCards[count].cardData.cardID;
				talentCards[count].cardData.movie = -1;
				talentCards[count].cardData.movieIdx = -1;
				talentCards[count].cardData.discardIdx = -1;
				count += 1;
			}
		}
		
		//shuffle
		utils.ShuffleCards(talentCards);
		
		//Sort talent cards by shuffle index
		talentCards = talentCards.OrderBy(go => go.cardData.deckIdx).ToArray();
		//move cards to the height to drop from
		float loc = 2f;
		Vector3 v = talentCards[0].transform.position;
		for (int i = 69; i >= 0; i--)
		{
			loc = loc + 0.1f;
			talentCards[i].transform.position = new Vector3(v.x, loc, v.z);
		}
		//turn off Kinematic to activate gravity
		//talentCards = talentCards.OrderByDescending(go => go.cardData.deckIdx).ToArray();
		foreach (Card item in talentCards)
		{
			item.GetComponent<Rigidbody>().isKinematic = false;
		}
		//talentCards = talentCards.OrderBy(go => go.cardData.deckIdx).ToArray();
	}

	void InitPlayers()
	{
		player = FindObjectsOfType<Player>();
		foreach(Player ply in player)
		{
			ply.hand = new int[] {-1, -1, -1, -1, -1, -1, -1};
			ply.nextHandIdx = 0;
			ply.transform.GetChild((int)PlayerDisplay.Fire).gameObject.SetActive(false);
		}
		//Sort players by playerID
		player = player.OrderBy(go => go.playerID).ToArray();
		playerCount = player.Length;
	}

	void LoadMovieTitles()
	{
		string filePath = Path.Combine(Application.streamingAssetsPath, "MovieTitles.json");
		if (File.Exists(filePath))
		{
			string dataJson = File.ReadAllText(filePath);
			movieTitles = JsonUtility.FromJson<MovieTitles>(dataJson);
		}
	}
	
	IEnumerator DealCards(int inDealer)
	{
		const int cardsDealt = 4;
		dealing = true;
		gNavCanvas.SetTicker("Dealing...");
		int[,] dealOrder = new int[4,4] {{1,2,3,0}, {2,3,0,1}, {3,0,1,2}, {0,1,2,3}}; //[inDealer, player]
		
		for (int i = 0; i < cardsDealt; i++)
		{
			for (int plyr = 0; plyr < player.Length; plyr++)
			{
				player[dealOrder[inDealer, plyr]].hand[player[dealOrder[inDealer, plyr]].nextHandIdx] = talentCards[curTalentCardsIdx].cardData.cardID;
				if(dealOrder[inDealer, plyr] != 0){talentCards[curTalentCardsIdx].GetComponent<Rigidbody>().isKinematic = false;}
				talentCards[curTalentCardsIdx].DealCardAnim(dealOrder[inDealer, plyr], i);
				talentCards[curTalentCardsIdx].cardData.deckIdx = -1;
				talentCards[curTalentCardsIdx].cardData.status = CardData.Status.Hand;
				talentCards[curTalentCardsIdx].cardData.hand = plyr;
				talentCards[curTalentCardsIdx].cardData.handIdx = i;
				player[dealOrder[inDealer, plyr]].nextHandIdx = i + 1;
				curTalentCardsIdx += 1;
				yield return new WaitForSeconds(0.6f);
			}
		}
		for (int plyr = 1; plyr < player.Length; plyr++)
		{
			player[plyr].AlignHand();
		}
		//lock the cards				
		for (int i = 0; i < cardsDealt; i++)
		{
			for (int plyr = 0; plyr < player.Length; plyr++)
			{
				if (plyr != thePlayerIndex)
				{
					GetTalentCardFromID(player[plyr].hand[i]).GetComponent<Rigidbody>().isKinematic = true;					
				}
			}
		}
		//set curPlayer
		curPlayer = dealer + 1;
		if(curPlayer >= player.Length){curPlayer = 0;}
		dealing = false;
	}
	
	IEnumerator ReshuffleCheck()
	{
		if (curTalentCardsIdx >= TalentCardCount)
		{
			shuffling = true;
			gNavCanvas.SetTicker("Shuffling Talent Cards...");
			yield return StartCoroutine("ReshuffleTalentCards");
		}
		if (curActionCardsIdx >= ActionCardCount)
		{
			shuffling = true;
			gNavCanvas.SetTicker("Shuffling Action Cards...");
			yield return StartCoroutine("ReshuffleActionCards");
		}
	}

	IEnumerator ReshuffleTalentCards()
	{
		if(shuffling)
		{
			int cnt = 0;
			//set discard cards to deck
			foreach(Card crd in talentCards)
			{
				if (crd.cardData.status	== CardData.Status.Discard)
				{
					crd.cardData.status = CardData.Status.Deck;
					cnt += 1;
				}
			}
			//put talent cards in deck to new deck
			int[] newDeck = new int[cnt];
			cnt = 0;
			foreach(Card crd in talentCards)
			{
				if (crd.cardData.status	== CardData.Status.Deck)
				{
					newDeck[cnt] = crd.cardData.cardID;
					cnt += 1;
				}
			}
			
			utils.ShuffleCards(newDeck);
			
			//Set the deck index to order of cards after shufffled
			cnt = 0;
			foreach (int item in newDeck)
			{
				GetTalentCardFromID(item).cardData.deckIdx = cnt;
				cnt += 1;
			}
			
			//Sort talent cards before drop by deck/shuffle index
			talentCards = talentCards.OrderByDescending(go => go.cardData.deckIdx).ToArray();
	
			//move cards to the height to drop from
			float loc = 2f;
			foreach (Card item in talentCards)
			{
				if (item.cardData.status ==	CardData.Status.Deck)
				{
					item.GetComponent<Rigidbody>().isKinematic = true;
					loc = loc + 0.1f;
					item.transform.position = new Vector3(3f,loc,0f);
					item.transform.DORotate(new Vector3(180f,180f,0f), 0f);
				}
			}
			
			//Drop Cards
			yield return new WaitForSeconds(1.5f); //2
			foreach (Card item in talentCards) 
			{
				if (item.cardData.status ==	CardData.Status.Deck)
				{
					item.GetComponent<Rigidbody>().isKinematic = false;
				}
			}

			yield return new WaitForSeconds(2f); //3
			
			//Lock Cards in the deck
			curTalentDiscardIdx = 0;
			cnt = 0;
			int deckCount = 0;

			foreach (Card item in talentCards)
			{
				if (item.cardData.status ==	CardData.Status.Deck)
				{
					item.GetComponent<Rigidbody>().isKinematic = true;
					deckCount += 1;
				}
				else
				{
					cnt += 1;
				}
			}
			
			//sort deck physically
			foreach (Card item in talentCards) 
			{
				if (item.cardData.status ==	CardData.Status.Deck)
				{
					item.transform.DOMoveY(((deckCount - item.cardData.deckIdx) * 0.01f) + 0.01f, 0f);
				}
			}

			//Sort talent cards in array by reverse deck\shuffle index
			talentCards = talentCards.OrderBy(go => go.cardData.deckIdx).ToArray();

			curTalentCardsIdx = cnt;
			yield return new WaitForSeconds(1f);
			shuffling = false;
		}
	}
	
	IEnumerator ReshuffleActionCards()
	{
		int cnt = 0;
		//set discard cards to deck
		foreach(Card crd in actionCards)
		{
			if (crd.cardData.status	== CardData.Status.Discard)
			{
				crd.cardData.status = CardData.Status.Deck;
				cnt += 1;
			}
		}
		
		//put action cards in deck to new deck
		int[] newDeck = new int[cnt + 1];
		cnt = 0;
		foreach(Card crd in actionCards)
		{
			if (crd.cardData.status	== CardData.Status.Deck)
			{
				newDeck[cnt] = crd.cardData.cardID;
				cnt += 1;
			}
		}
		
		utils.ShuffleCards(newDeck);
		
		//Set the deck index to order of cards after shufffled
		cnt =0;
		foreach (int item in newDeck)
		{
			actionCards[item].cardData.deckIdx = cnt;
			cnt += 1;
		}
		
		//Sort action cards by deck shuffle index
		actionCards = actionCards.OrderByDescending(go => go.cardData.deckIdx).ToArray();
		
		//move cards to the height to drop from
		float loc = 2f;
		foreach (Card item in actionCards)
		{
			if (item.cardData.status ==	CardData.Status.Deck)
			{
				item.GetComponent<Rigidbody>().isKinematic = true;
				loc = loc + 0.1f;
				item.transform.position = new Vector3(-0.9f,loc,0f);
				item.transform.DORotate(new Vector3(180f,180f,0f), 0f);
			}
		}
		
		//drop cards
		yield return new WaitForSeconds(1.5f);
		foreach (Card item in actionCards)
		{
			if (item.cardData.status ==	CardData.Status.Deck)
			{
				item.GetComponent<Rigidbody>().isKinematic = false;
			}
		}
		
		//wait until cards have fallen
		yield return new WaitForSeconds(2f);
		
		//stop gravity on all action cards
		foreach (Card item in actionCards)
		{
			item.GetComponent<Rigidbody>().isKinematic = true;
		}
		
		//move cards in both decks physically to match order in deck
		float nvalue = 0.02f;
		foreach (Card item in actionCards)
		{
			if (item.cardData.status ==	CardData.Status.Deck)
			{
				item.transform.position = new Vector3(-0.9f, nvalue, 0f);
				nvalue += .005f;				
			}
		}
		
		//reset tracking vars
		curActionCardsIdx = 0;
		curActionDiscardIdx = 0;

		yield return new WaitForSeconds(1f);
		shuffling = false;
	}
	
	
	public void CardDraw(Card inCard)
	{
		player[thePlayerIndex].PlayerDrawCard(inCard);
	}
	
	public void CardDiscard(Card inCard)
	{
		player[thePlayerIndex].PlayerDiscardCard(inCard);
	}

	public Card GetTalentCardFromID(int inCardID)
	{
		foreach (Card crd in talentCards)
		{
			if(crd.cardData.cardID == inCardID)
			{
				return crd;
			}
		}
		//an error if here
		return talentCards[0];
	}
	
	public int GetNextTalentCardID()
	{
		return talentCards[curTalentCardsIdx].cardData.cardID;
	}
	
	public PlayerAction GetCurPlayerAction()
	{
		return player[curPlayer].playerAction;
	}
	
	public int GetCurPlayerNextHandIdx()
	{
		return player[curPlayer].nextHandIdx;
	}
	
	public void SortTalentDiscard()
	{
		foreach(Card crd in talentCards)
		{
			if(crd.cardData.status == CardData.Status.Discard)
			{
				Vector3 loc = crd.transform.position;
				float yValue = 0.01f + (crd.cardData.discardIdx / 100f);
				if(loc.x < 4.5f || loc.x > 4.9f){loc.x = 4.7f;}
				if(loc.z < -0.2f || loc.z > 0.2f){loc.z = 0f;}
				crd.transform.position = new Vector3(4.7f, yValue, 0f);
				Vector3 rot = crd.transform.rotation.eulerAngles;
				crd.transform.DORotate(new Vector3(0, rot.y, 0), 0f);
			}
		}
	}
	
	private void DoTickerStartTurnMessage()
	{
		if(curPlayer == thePlayerIndex)
		{
			if(player[curPlayer].CanMakeMovie())
			{
				gNavCanvas.SetMovieButton(true);
				gNavCanvas.SetTicker("Your turn, You can Make a Movie, Draw an Action card, or Draw a Talent card.");
			}
			else
			{
				gNavCanvas.SetMovieButton(false);
				gNavCanvas.SetTicker("Your turn, Draw an Action or Talent card.");
			}
		}
		else
		{
			gNavCanvas.SetTicker(player[curPlayer].GetName() + "'s turn");	
		}
	}
	
	public void MakeMovieClicked()
	{
		player[thePlayerIndex].playerAction = PlayerAction.MakeMovie;
		player[thePlayerIndex].playerActed = true;
		gNavCanvas.SetMovieButton(false);
	}
	
	public string GetNewMovieTitle(string inType)
	{
		string retString = "";
		switch (inType)
		{
		case "Comedy":
			retString = movieTitles.comedy[Random.Range(0, movieTitles.comedy.Length - 1)];
			break;
		case "Drama":
			retString = movieTitles.drama[Random.Range(0, movieTitles.drama.Length - 1)];
			break;		
		case "Horror":
			retString = movieTitles.horror[Random.Range(0, movieTitles.horror.Length - 1)];
			break;		
		case "Musical":
			retString = movieTitles.musical[Random.Range(0, movieTitles.musical.Length - 1)];
			break;		
		case "Western":
			retString = movieTitles.western[Random.Range(0, movieTitles.western.Length - 1)];
			break;
		case "Action":
			retString = movieTitles.action[Random.Range(0, movieTitles.action.Length - 1)];
			break;
		}
		return retString;
	}

	public void MovieCardSelected(Card inCard)
	{
		player[curPlayer].MovieCardClicked(inCard);
	}
	
	public void MovieOKButton()
	{
		gMovieBackground.gameObject.SetActive(false);
		gNavCanvas.DisableMovieHud();
		player[curPlayer].playerActed = true;
	}

	public int GetPlayerHoldCardID()
	{
		return player[curPlayer].holdCardID;
	}
}
