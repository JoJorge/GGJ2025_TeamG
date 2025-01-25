using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.IO;

public class Player
{
    public string name;
    public int health;
}

public class JSONTest : MonoBehaviour {
    public Text text;

	// Use this for initialization
	void Start () {
        Player player1 = new Player();
        Player player2 = new Player();

        player1.name = "P1";
        player1.health = 100;
        player2.name = "P2";
        player2.health = 200;

        //JSON encoding
        string output;

        text.text += "\nEncoding: \n";
        output = JsonConvert.SerializeObject(player1, Formatting.Indented);
        File.WriteAllText("Player1.json", output);
        text.text += output + "\n";

        output = JsonConvert.SerializeObject(player2, Formatting.Indented);
        File.WriteAllText("Player2.json", output);
        text.text += output + "\n";

        Debug.Log(text.text);

        //Set for testing decoding.
        player1.name = "P3";
        player1.health = 300;
        player2.name = "P4";
        player2.health = 400;

        //JSON decoding
        string input = File.ReadAllText("Player1.json");

        text.text += "Decoding: \n";
        player1 = JsonConvert.DeserializeObject<Player>(input);
        text.text += player1.name + " " + player1.health + "\n";

        input = File.ReadAllText("Player2.json");
        player2 = JsonConvert.DeserializeObject<Player>(input);
        text.text += player2.name + " " + player2.health;
    }
}
