using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using TMPro;
using DG.Tweening;

// NOTE:
// from camera orientation
// X is right
// Y is up
// Z is forward into screen

/// <summary>
/// Main Controller where the game logic is handled and animated.
/// </summary>
public class MainController : MonoBehaviour
{

    public class Response
    {
        public string message { get; set; }
        public List<SlotData> slot_data { get; set; }
        public string status { get; set; }
    }

    public class SlotData
    {
        public List<List<int>> cascade_positions { get; set; }
        public string cur_mode { get; set; }
        public List<List<string>> display { get; set; }
        public string next_mode { get; set; }
        public int pay { get; set; }
        public int rounds_left { get; set; }
        public List<List<int>> scatter_win_positions { get; set; }
    }

    protected GameObject[,] symbolInstanceGrid;

    public Sequence currentSeq;

    public TextMeshProUGUI testTextDeleteMe;

    [SerializeField] private TMP_Dropdown dropdown;

    public GameObject symbolPrefab;
    public int rows = 5;
    public int cols = 5;
    public float XOffset;
    public float YOffset;
    public float gridSpacing;
    public float gridZ;

    private bool skippable = false;

    void Start()
    {
        // instantiate the grid
        symbolInstanceGrid = new GameObject[cols, rows];

        for (int col_idx = 0; col_idx < cols; col_idx++)
        {
            for (int row_idx = 0; row_idx < rows; row_idx++)
            {
                GameObject symbolAnchor = Instantiate(symbolPrefab);
                symbolAnchor.transform.position = getIdleWorldPos(col_idx, row_idx);
                symbolAnchor.transform.Find("SymbolObject").GetComponent<GenericSymbol>().StopAllAnims();

                symbolInstanceGrid[col_idx, row_idx] = symbolAnchor;
            }
        }

        // iterate through cheats dict to get keys and add as dropdown options
        Dictionary<string, List<int>> cheats = MainConfig.cheats;        
        foreach (string cheatname in cheats.Keys)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(cheatname));
        }
    }

    public IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    testTextDeleteMe.text = "Unknown error occurred: " + webRequest.error;
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    testTextDeleteMe.text = "Unknown error occurred: " + webRequest.error;
                    break;
                case UnityWebRequest.Result.Success:
                    //Debug.Log("Received: " + webRequest.downloadHandler.text);
                    Response response = JsonConvert.DeserializeObject<Response>(webRequest.downloadHandler.text);
                    testTextDeleteMe.text = "good";
                    StartCoroutine(playSequence(response));
                    break;
                // default
                default:
                    Debug.LogError("Unknown error occurred: " + webRequest.error);
                    testTextDeleteMe.text = "Unknown error occurred: " + webRequest.error;
                    break;
            }
        }
    }

    public IEnumerator PostRequest(string uri)
    {
        Dictionary<string, List<int>> body = new Dictionary<string, List<int>>();
        string cheatname = dropdown.options[dropdown.value].text;
        List<int> preset;
        try
        {
            preset = MainConfig.cheats[cheatname];
        }
        catch
        {
            preset = new List<int>();
        }
        body.Add("presets", preset);

        // hack needed to add body to post
        using (UnityWebRequest webRequest = UnityWebRequest.Put(uri, JsonConvert.SerializeObject(body)))
        {
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.method = "POST";
            yield return webRequest.SendWebRequest();
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    testTextDeleteMe.text = "Unknown error occurred: " + webRequest.error;
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    testTextDeleteMe.text = "Unknown error occurred: " + webRequest.error;
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log("Received: " + webRequest.downloadHandler.text);
                    Response response = JsonConvert.DeserializeObject<Response>(webRequest.downloadHandler.text);
                    testTextDeleteMe.text = "";
                    // Debug.Log(response.message);
                    StartCoroutine(playSequence(response));
                    break;
                // default
                default:
                    Debug.LogError("Unknown error occurred: " + webRequest.error);
                    testTextDeleteMe.text = "Unknown error occurred: " + webRequest.error;
                    break;
            }
        }
    }

    public void postClickTest()
    {
        // StartCoroutine(PostRequest("http://54.66.230.103/api/meow"));
        StartCoroutine(PostRequest("http://127.0.0.1:5000/api/spin"));
    }

    public void OnSpinClick()
    {
        StartCoroutine(GetRequest("http://54.66.230.103/api/data"));
    }

    public IEnumerator playSequence(Response responseObject)
    {
        Response resp = responseObject;

        float animDur = 1.0f;
        float delayDur = 2f;
        float explosionDur = 2.0f;

        // TODO: refactor this into buildable pattern classful thingo

        for (int i = 0; i < resp.slot_data.Count; i++)
        {
            List<List<int>> cascade_positions = resp.slot_data[i].cascade_positions;
            string cur_mode = resp.slot_data[i].cur_mode;
            List<List<string>> display = resp.slot_data[i].display;
            string next_mode = resp.slot_data[i].next_mode;
            int pay = resp.slot_data[i].pay;
            int rounds_left = resp.slot_data[i].rounds_left;
            List<List<int>> scatter_win_positions = resp.slot_data[i].scatter_win_positions;

            Sequence moveSeq = DOTween.Sequence();

            // setting the skippable thingo
            currentSeq = moveSeq;
            skippable = true;

            if (cur_mode == "base")
            {
                for (int col_idx = 0; col_idx < cols; col_idx++)
                {
                    for (int row_idx = 0; row_idx < rows; row_idx++)
                    {
                        // have to take a snapshot of the current idxs, can't use reference
                        int currentColIdx = col_idx;
                        int currentRowIdx = row_idx;

                        // get the symbol anchor
                        GameObject symbolAnchor = symbolInstanceGrid[currentColIdx, currentRowIdx];

                        Sequence subSeq = DOTween.Sequence();

                        // the symbols remaining should move behind the background
                        if (symbolAnchor != null)
                        {
                            Tween subTweenMoveBehind = symbolAnchor.transform.DOMove(symbolAnchor.transform.position + new Vector3(0, 0, 10), animDur).SetEase(Ease.InOutSine).OnComplete(() =>
                            {
                                // once in the background, change symbol texture
                                string symbolName = display[currentColIdx][currentRowIdx];
                                int id = MainConfig.SYMBOL_TYPE_MAPPING[symbolName];
                                symbolAnchor.transform.Find("SymbolObject").GetComponent<GenericSymbol>().ChangeToID(id);
                                // snap it to drop point
                                symbolAnchor.transform.position = getIdleWorldPos(currentColIdx, currentRowIdx) + new Vector3(0, 0, -15);
                            });
                            subSeq.Join(subTweenMoveBehind);
                        }
                        else
                        {
                            // make a new symbol behind background
                            symbolAnchor = Instantiate(symbolPrefab);
                            symbolAnchor.transform.position = getIdleWorldPos(currentColIdx, currentRowIdx) + new Vector3(0, 0, 10);
                            symbolInstanceGrid[currentColIdx, currentRowIdx] = symbolAnchor;
                            symbolAnchor.transform.Find("SymbolObject").GetComponent<GenericSymbol>().StopAllAnims();

                            string symbolName = display[currentColIdx][currentRowIdx];
                            int id = MainConfig.SYMBOL_TYPE_MAPPING[symbolName];
                            symbolAnchor.transform.Find("SymbolObject").GetComponent<GenericSymbol>().ChangeToID(id);

                            subSeq.AppendInterval(animDur);
                            subSeq.AppendCallback(() => {
                                symbolAnchor.transform.position = getIdleWorldPos(currentColIdx, currentRowIdx) + new Vector3(0, 0, -15);
                            });
                        }
                        

                        // then in should tween down to resting pos
                        Tween subTweenLand = symbolAnchor.transform.DOMove(getIdleWorldPos(currentColIdx, currentRowIdx), animDur).SetEase(Ease.InOutSine);

                        // subSeq.Join(subTweenLand);
                        subSeq.Append(subTweenLand);

                        moveSeq.Join(subSeq);
                    }
                }
            }
            else
            {
                // we're cascading wahoo
                Sequence cascadeSubSeq = DOTween.Sequence();

                // cascade what's left in the symbolInstanceGrid logically first and get them to move to their new positions
                for (int col_idx = 0; col_idx < cols; col_idx++)
                {
                    int swap_idx_ptr = rows - 1;
                    for (int row_idx = swap_idx_ptr; row_idx >= 0; row_idx--)
                    {
                        GameObject symbolAnchor = symbolInstanceGrid[col_idx, row_idx];
                        if (symbolAnchor != null)
                        {
                            // not null, let's move this symbolAnchor to the last row_idx_ptr
                            symbolInstanceGrid[col_idx, swap_idx_ptr] = symbolAnchor;
                            if (row_idx != swap_idx_ptr)
                            {
                                symbolInstanceGrid[col_idx, row_idx] = null;
                            }
                            swap_idx_ptr--;
                        }
                    }
                }

                for (int col_idx = 0; col_idx < cols; col_idx++)
                {
                    for (int row_idx = 0; row_idx < rows; row_idx++)
                    {
                        int currentColIdx = col_idx;
                        int currentRowIdx = row_idx;
                        GameObject symbolAnchor = symbolInstanceGrid[currentColIdx, currentRowIdx];
                        if (symbolAnchor != null)
                        {
                            Tween subTweenFall = symbolAnchor.transform.DOMove(getIdleWorldPos(currentColIdx, currentRowIdx), animDur).SetEase(Ease.InOutSine);
                            cascadeSubSeq.Join(subTweenFall);
                        }
                    }
                }

                moveSeq.Append(cascadeSubSeq);

                // then add new instances in whilst also dropping them down from the drop point to resting positions
                Sequence cascadeFillSubSeq = DOTween.Sequence();

                for (int col_idx = 0; col_idx < cols; col_idx++)
                {
                    for (int row_idx = 0; row_idx < rows; row_idx++)
                    {
                        if (symbolInstanceGrid[col_idx, row_idx] == null)
                        {
                            int currentColIdx = col_idx;
                            int currentRowIdx = row_idx;

                            // instantiate it at drop point
                            GameObject symbolAnchor = Instantiate(symbolPrefab);
                            symbolAnchor.transform.position = getIdleWorldPos(currentColIdx, currentRowIdx) + new Vector3(0, 0, -15);
                            symbolInstanceGrid[currentColIdx, currentRowIdx] = symbolAnchor;
                            symbolAnchor.transform.Find("SymbolObject").GetComponent<GenericSymbol>().StopAllAnims();

                            string symbolName = display[currentColIdx][currentRowIdx];
                            int id = MainConfig.SYMBOL_TYPE_MAPPING[symbolName];
                            symbolAnchor.transform.Find("SymbolObject").GetComponent<GenericSymbol>().ChangeToID(id);

                            Tween subTweenLand = symbolAnchor.transform.DOMove(getIdleWorldPos(currentColIdx, currentRowIdx), animDur).SetEase(Ease.InOutSine);
                            cascadeFillSubSeq.Join(subTweenLand);
                        }
                    }
                }

                moveSeq.Append(cascadeFillSubSeq);
            }
            
            yield return moveSeq.WaitForCompletion();

            Sequence moveSeq2 = DOTween.Sequence();
            currentSeq = moveSeq2;
            skippable = false;

            if (next_mode == "cascade")
            {
                // TODO: make a function for this
                moveSeq2.AppendInterval(delayDur);

                for (int posIdx = 0; posIdx < cascade_positions.Count; posIdx++)
                {
                    List<int> pos = cascade_positions[posIdx];
                    int colIdx = pos[0];
                    int rowIdx = pos[1];
                    GameObject symbolAnchor = symbolInstanceGrid[colIdx, rowIdx];
                    symbolAnchor.transform.Find("SymbolObject").GetComponent<GenericSymbol>().startTextureAnim();
                }
                moveSeq2.AppendCallback(() => {
                    // pop the cascade positions
                    for (int posIdx = 0; posIdx < cascade_positions.Count; posIdx++)
                    {
                        List<int> pos = cascade_positions[posIdx];
                        int colIdx = pos[0];
                        int rowIdx = pos[1];

                        GameObject symbolAnchor = symbolInstanceGrid[colIdx, rowIdx];
                        symbolAnchor.transform.Find("SymbolObject").GetComponent<GenericSymbol>().PlayExplosion();
                        
                        symbolAnchor.transform.Find("SymbolObject").GetComponent<GenericSymbol>().moveBack();
                        symbolAnchor.transform.Find("SymbolObject").GetComponent<GenericSymbol>().stopTextureAnim();
                    }
                });
                moveSeq2.AppendInterval(explosionDur);
                moveSeq2.AppendCallback(() => {
                    // delete cascade positions, inefficient o well
                    for (int posIdx = 0; posIdx < cascade_positions.Count; posIdx++)
                    {
                        List<int> pos = cascade_positions[posIdx];
                        int colIdx = pos[0];
                        int rowIdx = pos[1];
                        GameObject symbolAnchor = symbolInstanceGrid[colIdx, rowIdx];

                        symbolAnchor.transform.Find("SymbolObject").GetComponent<GenericSymbol>().StopExplosion();

                        Destroy(symbolAnchor);
                    }
                });

                
            }
            else
            {
                // play scatter wins if there are any
                moveSeq2.AppendInterval(delayDur);
                
                for (int posIdx = 0; posIdx < scatter_win_positions.Count; posIdx++)
                {
                    List<int> pos = scatter_win_positions[posIdx];
                    int colIdx = pos[0];
                    int rowIdx = pos[1];
                    GameObject symbolAnchor = symbolInstanceGrid[colIdx, rowIdx];
                    symbolAnchor.transform.Find("SymbolObject").GetComponent<GenericSymbol>().startTextureAnim();
                }
                moveSeq2.AppendCallback(() => {
                    // pop the cascade positions
                    for (int posIdx = 0; posIdx < cascade_positions.Count; posIdx++)
                    {
                        List<int> pos = cascade_positions[posIdx];
                        int colIdx = pos[0];
                        int rowIdx = pos[1];
                        GameObject symbolAnchor = symbolInstanceGrid[colIdx, rowIdx];
                        symbolAnchor.transform.Find("SymbolObject").GetComponent<GenericSymbol>().stopTextureAnim();
                    }
                });

            }

            yield return moveSeq2.WaitForCompletion();

            // have to add a delay here because tween libraries don't like destructions in coroutines
            yield return new WaitForSeconds(0.1f);
        }

        yield return null;
    }

    /// <summary>
    /// Gets the idle world pos for a given col and row.
    /// </summary>
    /// <param name="col_idx"></param>
    /// <param name="row_idx"></param>
    /// <returns></returns>
    public Vector3 getIdleWorldPos(int col_idx, int row_idx)
    {
        // -1 to flip the y axis as unity is y up, could also flip camera
        return new Vector3((col_idx - XOffset) * gridSpacing, (row_idx - YOffset) * gridSpacing * -1, gridZ);
    }

    // Skips the current sequence if possible.
    public void skipSequence()
    {
        if (skippable == true)
        {
            currentSeq.Complete();
        }
    }

}
