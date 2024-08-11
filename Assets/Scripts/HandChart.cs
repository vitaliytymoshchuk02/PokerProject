using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HandChartData
{
    public List<HandChartEntry> handChartList;

    [System.Serializable]
    public class HandChartEntry
    {
        public string key;
        public string value;
    }

    public Dictionary<string, string> ToDictionary()
    {
        Dictionary<string, string> handChart = new Dictionary<string, string>();
        foreach (HandChartEntry entry in handChartList)
        {
            handChart[entry.key] = entry.value;
        }
        return handChart;
    }
}

public class HandChart
{
    public Dictionary<string, string> handChart;
    private readonly int defaultNumberOfPlayers = 6;

    public HandChart()
    {
        LoadHandChart(defaultNumberOfPlayers);
    }

    public void LoadHandChart(int playersLeft)
    {
        switch (playersLeft)
        {
            case 2:
                {
                    TextAsset jsonFile = Resources.Load<TextAsset>("HandChart2Players");
                    if (jsonFile != null)
                    {
                        HandChartData data = JsonUtility.FromJson<HandChartData>(jsonFile.text);
                        handChart = data.ToDictionary();
                    }
                    else
                    {
                        Debug.LogError("Failed to load poker hand chart JSON file.");
                        handChart = new Dictionary<string, string>();
                    }
                    break;
                }
            case 3:
                {
                    TextAsset jsonFile = Resources.Load<TextAsset>("HandChart3Players");
                    if (jsonFile != null)
                    {
                        HandChartData data = JsonUtility.FromJson<HandChartData>(jsonFile.text);
                        handChart = data.ToDictionary();
                    }
                    else
                    {
                        Debug.LogError("Failed to load poker hand chart JSON file.");
                        handChart = new Dictionary<string, string>();
                    }
                    break;
                }
            case 4:
                {
                    TextAsset jsonFile = Resources.Load<TextAsset>("HandChart4Players");
                    if (jsonFile != null)
                    {
                        HandChartData data = JsonUtility.FromJson<HandChartData>(jsonFile.text);
                        handChart = data.ToDictionary();
                    }
                    else
                    {
                        Debug.LogError("Failed to load poker hand chart JSON file.");
                        handChart = new Dictionary<string, string>();
                    }
                    break;
                }
            case 5:
                {
                    TextAsset jsonFile = Resources.Load<TextAsset>("HandChart5Players");
                    if (jsonFile != null)
                    {
                        HandChartData data = JsonUtility.FromJson<HandChartData>(jsonFile.text);
                        handChart = data.ToDictionary();
                    }
                    else
                    {
                        Debug.LogError("Failed to load poker hand chart JSON file.");
                        handChart = new Dictionary<string, string>();
                    }
                    break;
                }
            default:
                {
                    TextAsset jsonFile = Resources.Load<TextAsset>("HandChart");
                    if (jsonFile != null)
                    {
                        HandChartData data = JsonUtility.FromJson<HandChartData>(jsonFile.text);
                        handChart = data.ToDictionary();
                    }
                    else
                    {
                        Debug.LogError("Failed to load poker hand chart JSON file.");
                        handChart = new Dictionary<string, string>();
                    }
                    break;
                }
        }

    }
}
