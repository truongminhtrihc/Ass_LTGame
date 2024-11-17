// using UnityEngine;
// using Unity.Netcode;
// using System.Collections.Generic;
// using SharpNeat.Phenomes;
// public class NEATPlayer : Player
// {
//     private IBlackBox neuralNetwork;

//     public NEATPlayer(string name, IBlackBox neuralNetwork) : base(name)
//     {
//         this.neuralNetwork = neuralNetwork;
//     }

//     public void Initialize(IBlackBox neuralNetwork)
//     {
//         this.neuralNetwork = neuralNetwork;
//     }

//     public void MakeDecision()
//     {
//         // Get inputs for the neural network
//         float[] inputs = GetInputs();
//         ISignalArray inputArr = neuralNetwork.InputSignalArray;
//         for (int i = 0; i < inputs.Length; i++)
//         {
//             inputArr[i] = inputs[i];
//         }

//         // Activate the neural network
//         neuralNetwork.Activate();

//         // Get outputs from the neural network
//         ISignalArray outputArr = neuralNetwork.OutputSignalArray;
//         float[] outputs = new float[outputArr.Length];
//         for (int i = 0; i < outputs.Length; i++)
//         {
//             outputs[i] = (float)outputArr[i];
//         }

//         // Implement decision logic based on outputs
//         // Example: if outputs[0] > 0.5, use a free jail card
//         if (isInJail && hasFreeJailCard && outputs[0] > 0.5f)
//         {
//             hasFreeJailCard = false;
//             isInJail = false;
//             jailTurns = 0;
//             Debug.Log(playerName + " used a free jail card.");
//         }

//         // Example: if outputs[1] > 0.5, buy the property
//         if (!GameManager.Instance.route.IsNodeOwned(currentPosition) && outputs[1] > 0.5f)
//         {
//             GameManager.Instance.BuyProperty(this);
//             Debug.Log(playerName + " bought the property at position " + currentPosition);
//         }
//     }

//     private float[] GetInputs()
//     {
//         // Check for null references and log them
//         if (GameManager.Instance == null)
//         {
//             Debug.LogError("GameManager.Instance is null");
//             return new float[0];
//         }

//         // Ensure propertyList is initialized
//         if (propertyList == null)
//         {
//             Debug.LogError("propertyList is null");
//             propertyList = new List<Property>();
//         }

//         // Log each property to identify the null reference
//         Debug.Log("money: " + money);
//         Debug.Log("livePreserver: " + livePreserver);
//         Debug.Log("propertyList.Count: " + propertyList.Count);
//         Debug.Log("isInJail: " + isInJail);
//         Debug.Log("hasFreeJailCard: " + hasFreeJailCard);
//         Debug.Log("jailTurns: " + jailTurns);
//         Debug.Log("currentPosition: " + currentPosition);
//         Debug.Log("monopolyGroupCount: " + monopolyGroupCount);
//         Debug.Log("GameManager.Instance.GetPlayerCount(): " + GameManager.Instance.GetPlayerCount());
//         Debug.Log("GameManager.Instance.GetCurrentTurn(): " + GameManager.Instance.GetCurrentTurn());
//         // Get inputs for the neural network
//         return new float[]
//         {
//             money,
//             livePreserver,
//             propertyList.Count,
//             isInJail ? 1 : 0,
//             hasFreeJailCard ? 1 : 0,
//             jailTurns,
//             currentPosition,
//             monopolyGroupCount,
//             GameManager.Instance.GetPlayerCount(), // number of players in the game
//             GameManager.Instance.GetCurrentTurn() // current turn number
//         };
//     }
// }
