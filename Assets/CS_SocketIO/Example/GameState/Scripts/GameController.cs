using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject GameContainer;
    [SerializeField] private Transform PlayersContainer;
    [SerializeField] private Transform CoinsContainer;
    [SerializeField] Transform MissilesContainer;

    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject CoinPrefab;
    [SerializeField] GameObject MissilePrefab;

    private GameState State;
    private Dictionary<string, Transform> PlayersToRender;
    private Dictionary<string, Transform> CoinsToRender;
    private Dictionary<string, Transform> MissilesToRender;

    private void Awake()
    {
        PlayersToRender = new Dictionary<string, Transform>();
        CoinsToRender = new Dictionary<string, Transform>();
        MissilesToRender = new Dictionary<string, Transform>();
    }

    internal void StartGame(GameState state)
    {
        GameObject.Find("PanelConnect").SetActive(false);
        GameContainer.SetActive(true);

        foreach (Player player in state.Players)
        {
            InstantiatePlayer(player);
        }

        var Socket = NetworkController._Instance.Socket;

        InputController._Instance.onAxisChange += (axis) => { Socket.Emit("move", axis); };
        InputController._Instance.onFireMissile += () =>
        {
            // Obtener la posición del primer jugador en la lista (puedes ajustarlo según tu lógica)
            Vector2 playerPosition = state.Players.FirstOrDefault()?.Position ?? Vector2.zero;

            string missileId = Guid.NewGuid().ToString();
            // Lógica para spawnear un misil en la posición del jugador
            SpawnMissile(missileId,playerPosition);
        };

        State = state;
        Socket.On("updateState", UpdateState);
    }

    internal void SpawnMissile(string id, Vector2 position)
    {
        // Lógica para instanciar un nuevo misil en Unity
        GameObject missileGameObject = Instantiate(MissilePrefab, MissilesContainer);
        missileGameObject.transform.position = position;
        missileGameObject.GetComponent<GameMissile>().Id = id;

        // Agregar el misil al diccionario de misiles
        MissilesToRender[id] = missileGameObject.transform;

        // Enviar un mensaje al servidor para indicar que se ha disparado un nuevo misil
        NetworkController._Instance.Socket.Emit("spawnMissile", new { Id = id, Position = position });
    }

    private void InstantiatePlayer(Player player)
    {
        GameObject playerGameObject = Instantiate(PlayerPrefab, PlayersContainer);
        playerGameObject.transform.position = new Vector2(player.x, player.y);
        playerGameObject.GetComponent<GamePlayer>().Id = player.Id;
        playerGameObject.GetComponent<GamePlayer>().Username = player.Username;

        
        PlayersToRender[player.Id] = playerGameObject.transform;
    }

    private void UpdateState(string json)
    {
        GameStateData jsonData = JsonUtility.FromJson<GameStateData>(json);
        State = jsonData.State;
    }

    internal void NewPlayer(string id, string username)
    {
        InstantiatePlayer(new Player { Id = id, Username = username });
    }
    internal void FireMissile()
    {
        // Enviar un mensaje al servidor para indicar que se quiere disparar un misil
        NetworkController._Instance.Socket.Emit("spawnMissile", "");
    }
    void Update()
    {
        if (State != null)
        {
            foreach (Player player in State.Players)
            {
                if (PlayersToRender.ContainsKey(player.Id))
                {
                    PlayersToRender[player.Id].position = new Vector2(player.x, player.y);
                }
                else
                {
                    InstantiatePlayer(player);
                }
              
            }
            var plarersToDelete = PlayersToRender.Where(item => !State.Players.Any(player => player.Id == item.Key)).ToList();
            foreach (var playerItem in plarersToDelete)
            {
                Destroy(playerItem.Value.gameObject);
                PlayersToRender.Remove(playerItem.Key);
            }
            foreach (Coin coin in State.Coins)
            {
                if (CoinsToRender.ContainsKey(coin.Id))
                {
                    CoinsToRender[coin.Id].position = new Vector2(coin.x, coin.y);
                }
                else
                {
                    InstantiateCoin(coin);
                }
            }
            var coinsToDelete = CoinsToRender.Where(item => !State.Coins.Any(coin => coin.Id == item.Key)).ToList();

            foreach (var coinItem in coinsToDelete)
            {
                Destroy(coinItem.Value.gameObject);
                CoinsToRender.Remove(coinItem.Key);
            }

        }
    }
    private void InstantiateCoin(Coin coin)
    {
        GameObject coinGameObject = Instantiate(CoinPrefab, CoinsContainer);
        coinGameObject.transform.position = new Vector2(coin.x, coin.y);
        coinGameObject.GetComponent<GameCoin>().Id = coin.Id;

        CoinsToRender[coin.Id] = coinGameObject.transform;
    }
}

[Serializable]
public class GameStateData
{
    public GameState State;
}
