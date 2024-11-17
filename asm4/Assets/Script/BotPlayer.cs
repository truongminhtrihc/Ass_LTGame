using UnityEngine;
using System;
using System.Linq; // Ensure this namespace is included
using Unity.Netcode;
using System.Collections.Generic;

public enum ActionType
{
    BuyProperty,
    Pass
    // Add more actions as needed, e.g., BuildHouse, Trade, etc.
}

public class GameState
{
    public int CurrentPlayerID { get; set; }
    public List<PlayerState> Players { get; set; }
    public List<PropertyState> Properties { get; set; }
    public int CurrentPosition { get; set; }
    public float PlayerFunds { get; set; }
    // Add other necessary game state details

    public GameState Clone()
    {
        return new GameState
        {
            CurrentPlayerID = this.CurrentPlayerID,
            Players = new List<PlayerState>(this.Players.ConvertAll(p => p.Clone())),
            Properties = new List<PropertyState>(this.Properties.ConvertAll(p => p.Clone())),
            CurrentPosition = this.CurrentPosition,
            PlayerFunds = this.PlayerFunds
            // Clone other details as needed
        };
    }

    public bool CanTakeAction()
    {
        // Example: Check if there's a property to buy
        return Properties.Exists(p => !p.IsOwned && p.PropertyID == CurrentPosition);
    }
}

public class PlayerState
{
    public string PlayerName { get; set; }
    public int PlayerID { get; set; }
    public float Funds { get; set; }
    public List<Property> OwnedProperties { get; set; }
    public bool IsInJail { get; set; }
    // Add other player-specific details

    public PlayerState Clone()
    {
        return new PlayerState
        {
            PlayerName = this.PlayerName,
            PlayerID = this.PlayerID,
            Funds = this.Funds,
            OwnedProperties = new List<Property>(this.OwnedProperties),
            IsInJail = this.IsInJail
            // Clone other properties as needed
        };
    }
}

public class PropertyState
{
    public string PropertyName { get; set; }
    public int PropertyID { get; set; }
    public bool IsOwned { get; set; }
    public int OwnerID { get; set; }
    public int Price { get; set; }
    public int Group { get; set; }

    public PropertyState Clone()
    {
        return new PropertyState
        {
            PropertyName = this.PropertyName,
            PropertyID = this.PropertyID,
            IsOwned = this.IsOwned,
            OwnerID = this.OwnerID,
            Price = this.Price,
            Group = this.Group
        };
    }
}

public class MCTSParameters
{
    public int MaxSimulations { get; set; } = 1000;
    public float ExplorationConstant { get; set; } = 1.414f; // √2 is a common choice
}

public class MCTSNode
{
    public GameState State { get; private set; }
    public MCTSNode Parent { get; private set; }
    public List<MCTSNode> Children { get; private set; }
    public ActionType? Action { get; private set; }

    public int VisitCount { get; private set; }
    public float TotalReward { get; private set; }

    public MCTSNode(GameState state, ActionType? action = null, MCTSNode parent = null)
    {
        State = state;
        Action = action;
        Parent = parent;
        Children = new List<MCTSNode>();
        VisitCount = 0;
        TotalReward = 0.0f;
    }

    public bool IsFullyExpanded()
    {
        // All possible actions have been expanded
        return Children.Count >= Enum.GetValues(typeof(ActionType)).Length;
    }

    public float GetAverageReward()
    {
        return VisitCount == 0 ? 0 : TotalReward / VisitCount;
    }

    public void UpdateStats(float reward)
    {
        VisitCount++;
        TotalReward += reward;
    }
}

public class MCTS
{
    private MCTSParameters _parameters;

    public MCTS(MCTSParameters parameters)
    {
        _parameters = parameters;
    }

    public ActionType Decide(GameState rootState)
    {
        MCTSNode root = new MCTSNode(rootState);

        for (int i = 0; i < _parameters.MaxSimulations; i++)
        {
            // Selection
            MCTSNode node = Selection(root);

            // Expansion
            if (node.State.CanTakeAction())
            {
                node = Expansion(node);
            }

            // Simulation
            float reward = Simulation(node.State);

            // Backpropagation
            Backpropagate(node, reward);
        }

        // Choose the action with the highest average reward
        if (root.Children.Count == 0)
        {
            // If no children, default to Pass
            return ActionType.Pass;
        }

        return root.Children.OrderByDescending(c => c.GetAverageReward()).First().Action.Value;
    }

    private MCTSNode Selection(MCTSNode node)
    {
        while (node.Children.Count > 0)
        {
            node = BestUCT(node);
        }
        return node;
    }

    private MCTSNode BestUCT(MCTSNode node)
    {
        return node.Children.OrderByDescending(c =>
            c.GetAverageReward() + _parameters.ExplorationConstant * Mathf.Sqrt(Mathf.Log(node.VisitCount + 1) / (c.VisitCount + 1))
        ).First();
    }

    private MCTSNode Expansion(MCTSNode node)
    {
        // Get all possible actions from the current state
        var possibleActions = Enum.GetValues(typeof(ActionType)).Cast<ActionType>().ToList();

        // Find actions that have not been expanded yet
        var triedActions = node.Children
                                .Select(c => c.Action)
                                .Where(a => a.HasValue) // Filter out nulls
                                .Select(a => a.Value)
                                .ToList();

        var untriedActions = possibleActions.Except(triedActions).ToList();

        if (untriedActions.Count == 0)
        {
            return node; // Fully expanded
        }

        // Select a random untried action
        ActionType action = untriedActions[UnityEngine.Random.Range(0, untriedActions.Count)];

        // Apply the action to get a new game state
        GameState newState = ApplyAction(node.State, action);

        // Create a new child node
        MCTSNode child = new MCTSNode(newState, action, node);
        node.Children.Add(child);
        return child;
    }

    private float Simulation(GameState state)
    {
        // Clone the state to avoid modifying the actual game state
        GameState simState = state.Clone();

        // Simulate until a termination condition or a certain number of turns
        int simulationTurns = 10; // Adjust based on desired simulation depth
        for (int i = 0; i < simulationTurns; i++)
        {
            // Determine the current player
            int currentPlayerID = simState.CurrentPlayerID;
            PlayerState currentPlayer = simState.Players.FirstOrDefault(p => p.PlayerID == currentPlayerID);

            if (currentPlayer == null)
            {
                Debug.LogError("Simulation Error: Current player not found.");
                break;
            }

            // Simulate dice roll
            int diceRoll = UnityEngine.Random.Range(2, 13); // 2 to 12

            // Move player (Assuming you have a method to handle player movement)
            simState.CurrentPosition = (simState.CurrentPosition + diceRoll) % simState.Properties.Count;

            // Get the property landed on
            PropertyState landedProperty = simState.Properties.FirstOrDefault(p => p.PropertyID == simState.CurrentPosition);
            if (landedProperty == null)
            {
                Debug.LogError($"Simulation Error: Property at position {simState.CurrentPosition} not found.");
                continue;
            }

            // Decide to buy or pass based on some heuristic (e.g., simple random choice)
            if (!landedProperty.IsOwned && currentPlayer.Funds >= landedProperty.Price)
            {
                // Simple heuristic: 50% chance to buy
                bool decideToBuy = UnityEngine.Random.value > 0.5f;
                if (decideToBuy)
                {
                    landedProperty.IsOwned = true;
                    landedProperty.OwnerID = currentPlayer.PlayerID;
                    currentPlayer.Funds -= landedProperty.Price;

                    // Add property to player's owned properties
                    Property purchasedProperty = new Property(landedProperty.PropertyName, landedProperty.Price, /* group */ 0); // Assume group is 0 for simplicity
                    currentPlayer.OwnedProperties.Add(purchasedProperty);
                }
            }

            // Switch to next player (Assuming you have logic to determine the next player)
            // For simplicity, we'll just break after one turn
            break;
        }

        // Define how to calculate the reward based on the simulation
        // For example, higher funds yield higher rewards
        PlayerState botPlayer = simState.Players.FirstOrDefault(p => p.PlayerID == state.CurrentPlayerID);
        return botPlayer != null ? botPlayer.Funds : 0f;
    }

    private void Backpropagate(MCTSNode node, float reward)
    {
        while (node != null)
        {
            node.UpdateStats(reward);
            node = node.Parent;
        }
    }

    private GameState ApplyAction(GameState state, ActionType action)
    {
        // Implement how actions transform the game state
        // For example, if action is BuyProperty, deduct funds and assign property to the bot
        GameState newState = state.Clone();

        switch (action)
        {
            case ActionType.BuyProperty:
                PropertyState propertyToBuy = newState.Properties.FirstOrDefault(p => p.PropertyID == newState.CurrentPosition);
                if (propertyToBuy != null && !propertyToBuy.IsOwned && newState.PlayerFunds >= propertyToBuy.Price)
                {
                    propertyToBuy.IsOwned = true;
                    propertyToBuy.OwnerID = newState.CurrentPlayerID;
                    newState.PlayerFunds -= propertyToBuy.Price;

                    // Add property to player's owned properties
                    PlayerState player = newState.Players.FirstOrDefault(p => p.PlayerID == newState.CurrentPlayerID);
                    if (player != null)
                    {
                        // Assuming you have a method to get Property based on PropertyID
                        NodeInfo node = GameManager.Instance.route.properties.FirstOrDefault(p => p.ID == propertyToBuy.PropertyID);
                        if (node != null)
                        {
                            Property actualProperty = new Property(node.name, node.price, node.group);
                            player.OwnedProperties.Add(actualProperty);
                        }
                    }
                }
                break;

            case ActionType.Pass:
                // Do nothing
                break;
        }

        // Implement other state transitions as needed

        return newState;
    }
}

public class BotPlayer : Player
{
    private MCTS _mcts;
    private MCTSParameters _mctsParameters;

    // Initialization without Neural Network
    public BotPlayer(string name) : base(name)
    {
        // Initialize MCTS with desired parameters
        _mctsParameters = new MCTSParameters
        {
            MaxSimulations = 500,
            ExplorationConstant = 1.414f
        };
        _mcts = new MCTS(_mctsParameters);
    }

    // If you decide to keep Neural Network later, retain the following
    /*
    private IBlackBox neuralNetwork;

    public BotPlayer(string name, IBlackBox neuralNetwork) : base(name)
    {
        this.neuralNetwork = neuralNetwork;
    }

    public void Initialize(IBlackBox neuralNetwork)
    {
        this.neuralNetwork = neuralNetwork;
    }
    */

    // Initialize the BotPlayer
    private void Awake()
    {
        InitializeBotPlayer();
    }

    private void InitializeBotPlayer()
    {
        // Initialize base Player properties if not already initialized
        if (propertyList == null)
            propertyList = new List<Property>();

        // Initialize MCTS parameters
        _mctsParameters = new MCTSParameters
        {
            MaxSimulations = 500,
            ExplorationConstant = 1.414f
        };

        // Initialize MCTS
        _mcts = new MCTS(_mctsParameters);
    }
    public ActionType MakeDecision(GameState currentState)
    {
        if (currentState == null)
        {
            Debug.LogError("Current game state is null. Defaulting to Pass.");
            return ActionType.Pass;
        }

        // Use MCTS to decide the next action
        ActionType decision = _mcts.Decide(currentState);
        Debug.Log($"{playerName} decided to {decision}.");
        return decision;
    }

    private void ExecuteAction(ActionType action, GameState currentState)
    {
        PropertyState currentProperty = currentState.Properties.FirstOrDefault(p => p.PropertyID == currentPosition);

        if (currentProperty == null)
        {
            Debug.LogError("Current property is null. Cannot execute action.");
            return;
        }

        switch (action)
        {
            case ActionType.BuyProperty:
                if (money >= currentProperty.Price && !currentProperty.IsOwned && currentProperty.Group != 0)
                {
                    BuyProperty(currentProperty, currentState);
                    Debug.Log($"{playerName} decided to buy {currentProperty.PropertyName}");
                }
                else
                {
                    Debug.Log($"{playerName} wanted to buy {currentProperty.PropertyName} but lacks funds or property is already owned.");
                }
                break;

            case ActionType.Pass:
                Debug.Log($"{playerName} decided not to buy {currentProperty.PropertyName}");
                // Implement any pass-specific logic if necessary
                break;
        }
    }

    private void BuyProperty(PropertyState property, GameState currentState)
    {
        // Update game state accordingly
        property.IsOwned = true;
        property.OwnerID = this.playerID;
        this.money -= property.Price;

        // Retrieve the corresponding NodeInfo
        NodeInfo node = GameManager.Instance.route.properties.FirstOrDefault(p => p.ID == property.PropertyID);
        if (node != null)
        {
            // Create a new Property instance based on NodeInfo
            Property actualProperty = new Property(node.name, node.price, node.group);

            // Retrieve the player's state from the current game state
            PlayerState player = currentState.Players.FirstOrDefault(p => p.PlayerID == this.playerID);
            if (player != null)
            {
                player.OwnedProperties.Add(actualProperty);
            }
            else
            {
                Debug.LogError($"Player with ID {this.playerID} not found in GameState.");
            }
        }
        else
        {
            Debug.LogError($"Node with ID {property.PropertyID} not found.");
        }

        // Fire events or update UI as needed
    }
}