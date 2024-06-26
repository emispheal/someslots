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

    // NOTE: needs to be in snake case to automatically convert from json
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

    protected Sequence currentSeq;

    [SerializeField] protected TextMeshProUGUI serverStatusText;

    [SerializeField] private TMP_Dropdown dropdown;

    [SerializeField] protected TextMeshProUGUI curGamePay;
    private int curPay;

    private int curBet;

    [SerializeField] protected GameObject symbolPrefab;
    [SerializeField] protected int rows = MainConfig.ROWS;
    [SerializeField] protected int cols = MainConfig.COLS;
    [SerializeField] protected float XOffset;
    [SerializeField] protected float YOffset;
    [SerializeField] protected float gridSpacing;
    [SerializeField] protected float gridZ;

    [SerializeField] protected float animDur = 1.0f;
    [SerializeField] protected float delayDur = 1f;
    [SerializeField] protected float explosionDur = 1.0f;

    private bool skippable = false;
    private bool inPlay = false;

    void Start()
    {
        // instantiate the grid
        symbolInstanceGrid = new GameObject[cols, rows];

        for (int col_idx = 0; col_idx < cols; col_idx++)
        {
            for (int row_idx = 0; row_idx < rows; row_idx++)
            {
                GameObject symbolAnchor = Instantiate(symbolPrefab);
                symbolAnchor.transform.position = GetIdleWorldPos(col_idx, row_idx);
                symbolAnchor.transform.Find(MainConfig.SYMBOL_OBJECT).GetComponent<CascadeSymbol>().StopAllAnims();

                symbolInstanceGrid[col_idx, row_idx] = symbolAnchor;
            }
        }

        // iterate through cheats dict to get keys and add as dropdown options
        Dictionary<string, List<int>> cheats = MainConfig.CHEATS;        
        foreach (string cheatname in cheats.Keys)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(cheatname));
        }

        ResetPay();

        // initial bet
        SyncBet(MainConfig.BET_MULTIPLES[0]);
    }

    // Starts a request for a spin sequence when the spin button is clicked
    public void OnSpinClick()
    {
        // if (inPlay == false) StartCoroutine(PostRequest("http://127.0.0.1:5000/api/spin"));
        if (inPlay == false) StartCoroutine(PostRequest(MainConfig.SERVER_URL));
    }

    // Sends a POST request to the server and handles the response
    // plays game sequence if the request was successful
    public IEnumerator PostRequest(string uri)
    {
        Dictionary<string, List<int>> body = new Dictionary<string, List<int>>();
        string cheatname = dropdown.options[dropdown.value].text;
        List<int> preset;
        try
        {
            preset = MainConfig.CHEATS[cheatname];
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
                    serverStatusText.text = "Server error occurred: " + webRequest.error;
                    break;

                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    serverStatusText.text = "Server error occurred: " + webRequest.error;
                    break;

                case UnityWebRequest.Result.Success:
                    Debug.Log("Received: " + webRequest.downloadHandler.text);
                    Response response = JsonConvert.DeserializeObject<Response>(webRequest.downloadHandler.text);
                    serverStatusText.text = "";
                    StartCoroutine(PlaySequence(response));
                    break;

                default:
                    Debug.LogError("Unknown error occurred: " + webRequest.error);
                    serverStatusText.text = "Unknown error occurred: " + webRequest.error;
                    break;
            }
        }
    }

    // handles the response and plays the game sequence
    // animations and toggleables are controlled here
    public IEnumerator PlaySequence(Response responseObject)
    {
        Response resp = responseObject;
        ResetPay();
        inPlay = true;

        for (int i = 0; i < resp.slot_data.Count; i++)
        {
            // unpacking the response
            List<List<int>> cascadePositions = resp.slot_data[i].cascade_positions;
            string curMode = resp.slot_data[i].cur_mode;
            List<List<string>> display = resp.slot_data[i].display;
            string nextMode = resp.slot_data[i].next_mode;
            int pay = resp.slot_data[i].pay;
            int roundsLeft = resp.slot_data[i].rounds_left;
            List<List<int>> scatterWinPositions = resp.slot_data[i].scatter_win_positions;

            // setting whether this part of play is skippable and current sequence
            Sequence settingSeq = DOTween.Sequence();
            currentSeq = settingSeq;
            skippable = true;

            if (curMode == "base")
            {
                // current mode is base, so clean out the remaining/full board and add new symbols
                for (int col_idx = 0; col_idx < cols; col_idx++)
                {
                    for (int row_idx = 0; row_idx < rows; row_idx++)
                    {
                        // have to take a snapshot of the current idxs, can't use reference due to it being incremented
                        int currentColIdx = col_idx, currentRowIdx = row_idx;
                        GameObject symbolAnchor = symbolInstanceGrid[currentColIdx, currentRowIdx];

                        Sequence subSeq = DOTween.Sequence();

                        // the symbols remaining should move behind the background
                        if (symbolAnchor != null)
                        {
                            Tween subTweenMoveBehind = symbolAnchor.transform.DOMove(GetMoveBackPos(currentColIdx, currentRowIdx), animDur).SetEase(Ease.InOutSine).OnComplete(() =>
                            {
                                // once in the background, change symbol texture
                                ChangeSymbolTexture(symbolAnchor, GetIDFromDisplay(display, currentColIdx, currentRowIdx));

                                // snap it to drop point
                                symbolAnchor.transform.position = GetDropPointPos(currentColIdx, currentRowIdx);
                            });
                            subSeq.Join(subTweenMoveBehind);
                        }
                        else
                        {
                            // make a new symbol behind background
                            symbolAnchor = Instantiate(symbolPrefab);
                            symbolAnchor.transform.position = GetMoveBackPos(currentColIdx, currentRowIdx);
                            symbolInstanceGrid[currentColIdx, currentRowIdx] = symbolAnchor;
                            symbolAnchor.transform.Find(MainConfig.SYMBOL_OBJECT).GetComponent<CascadeSymbol>().StopAllAnims();

                            ChangeSymbolTexture(symbolAnchor, GetIDFromDisplay(display, currentColIdx, currentRowIdx));
                            
                            subSeq.AppendInterval(animDur);
                            subSeq.AppendCallback(() => {
                                symbolAnchor.transform.position = GetDropPointPos(currentColIdx, currentRowIdx);
                            });
                        }
                        
                        // then in should tween down to resting pos
                        Tween subTweenLand = symbolAnchor.transform.DOMove(GetIdleWorldPos(currentColIdx, currentRowIdx), animDur).SetEase(Ease.InOutSine);
                        subSeq.Append(subTweenLand);

                        settingSeq.Join(subSeq);
                    }
                }
            }
            else
            {
                // we're cascading wahoo
                Sequence cascadeSubSeq = DOTween.Sequence();

                // cascade what's left in the symbolInstanceGrid logically first
                CascadeSymbolInstanceGrid();

                // now get them to actually cascade/fall tween to their new resting positions
                for (int colIdx = 0; colIdx < cols; colIdx++)
                {
                    for (int rowIdx = 0; rowIdx < rows; rowIdx++)
                    {
                        int currentColIdx = colIdx, currentRowIdx = rowIdx;
                        GameObject symbolAnchor = symbolInstanceGrid[currentColIdx, currentRowIdx];
                        if (symbolAnchor != null)
                        {
                            Tween subTweenFall = symbolAnchor.transform.DOMove(GetIdleWorldPos(currentColIdx, currentRowIdx), animDur).SetEase(Ease.InOutSine);
                            cascadeSubSeq.Join(subTweenFall);
                        }
                    }
                }

                settingSeq.Append(cascadeSubSeq);

                // then add new instances in whilst also dropping them down from the drop point to resting positions
                Sequence cascadeFillSubSeq = DOTween.Sequence();

                for (int colIdx = 0; colIdx < cols; colIdx++)
                {
                    for (int rowIdx = 0; rowIdx < rows; rowIdx++)
                    {
                        if (symbolInstanceGrid[colIdx, rowIdx] == null)
                        {
                            int currentColIdx = colIdx, currentRowIdx = rowIdx;

                            // instantiate it at drop point and set correct texture
                            GameObject symbolAnchor = Instantiate(symbolPrefab);
                            symbolAnchor.transform.position = GetDropPointPos(currentColIdx, currentRowIdx);
                            symbolInstanceGrid[currentColIdx, currentRowIdx] = symbolAnchor;
                            GetSymbolScript(symbolAnchor).StopAllAnims();
                            ChangeSymbolTexture(symbolAnchor, GetIDFromDisplay(display, currentColIdx, currentRowIdx));

                            // drop tween to resting position
                            Tween subTweenLand = symbolAnchor.transform.DOMove(GetIdleWorldPos(currentColIdx, currentRowIdx), animDur).SetEase(Ease.InOutSine);
                            cascadeFillSubSeq.Join(subTweenLand);
                        }
                    }
                }

                settingSeq.Append(cascadeFillSubSeq);
            }
            
            yield return settingSeq.WaitForCompletion();

            Sequence winSeq = DOTween.Sequence();
            currentSeq = winSeq;
            skippable = false;

            if (nextMode == "cascade")
            {
                // symbols are set, pay can be derived already, so add them to ui
                AddPay(pay);

                // play the wins before exploding them by animating the textures
                winSeq.AppendInterval(delayDur);
                for (int posIdx = 0; posIdx < cascadePositions.Count; posIdx++)
                {
                    List<int> pos = cascadePositions[posIdx];
                    int colIdx = pos[0], rowIdx = pos[1];
                    GameObject symbolAnchor = symbolInstanceGrid[colIdx, rowIdx];
                    GetSymbolScript(symbolAnchor).StartTextureAnim();
                }

                // after win animations pop/explode the cascade positions
                winSeq.AppendCallback(() => {
                    for (int posIdx = 0; posIdx < cascadePositions.Count; posIdx++)
                    {
                        List<int> pos = cascadePositions[posIdx];
                        int colIdx = pos[0], rowIdx = pos[1];

                        GameObject symbolAnchor = symbolInstanceGrid[colIdx, rowIdx];
                        GetSymbolScript(symbolAnchor).PlayExplosion();
                        GetSymbolScript(symbolAnchor).HideFromView();
                        GetSymbolScript(symbolAnchor).StopTextureAnim();
                    }
                });

                // wait for the explosion to play
                winSeq.AppendInterval(explosionDur);

                // then delete the cascade positions
                winSeq.AppendCallback(() => {
                    // delete cascade positions, inefficient o well
                    for (int posIdx = 0; posIdx < cascadePositions.Count; posIdx++)
                    {
                        List<int> pos = cascadePositions[posIdx];
                        int colIdx = pos[0], rowIdx = pos[1];
                        GameObject symbolAnchor = symbolInstanceGrid[colIdx, rowIdx];

                        GetSymbolScript(symbolAnchor).StopExplosion();
                        Destroy(symbolAnchor);
                    }
                });
                
            }
            else
            {
                // play scatter wins if there are any
                winSeq.AppendInterval(delayDur);
                for (int posIdx = 0; posIdx < scatterWinPositions.Count; posIdx++)
                {
                    List<int> pos = scatterWinPositions[posIdx];
                    int colIdx = pos[0], rowIdx = pos[1];
                    GameObject symbolAnchor = symbolInstanceGrid[colIdx, rowIdx];
                    GetSymbolScript(symbolAnchor).StartTextureAnim();

                    winSeq.AppendCallback(() => {
                        symbolAnchor.transform.Find(MainConfig.SYMBOL_OBJECT).GetComponent<CascadeSymbol>().StopTextureAnim();
                    });
                }
            }

            yield return winSeq.WaitForCompletion();

            // have to add a delay here because tween libraries don't like destructions in coroutines
            yield return new WaitForSeconds(0.1f);
        }

        inPlay = false;

        yield return null;
    }

    // Gets the idle world pos/static position for a given col and row idxs.
    public Vector3 GetIdleWorldPos(int colIdx, int rowIdx)
    {
        // -1 to flip the y axis as unity is y up, could also flip camera
        return new Vector3((colIdx - XOffset) * gridSpacing, (rowIdx - YOffset) * gridSpacing * -1, gridZ);
    }

    // Where new symbols are supposed to be instantiated at above the grid
    public Vector3 GetDropPointPos(int colIdx, int rowIdx)
    {
        return GetIdleWorldPos(colIdx, rowIdx) + new Vector3(0, 0, -15);
    }

    // Where symbols are supposed to land at after "sinking" behind the background
    public Vector3 GetMoveBackPos(int colIdx, int rowIdx)
    {
        return GetIdleWorldPos(colIdx, rowIdx) + new Vector3(0, 0, 10);
    }

    // Skips the current sequence if possible.
    public void SkipSequence()
    {
        if (skippable == true) currentSeq.Complete();
    }

    // Adds pay and syncs it to the UI
    private void AddPay(int amount)
    {
        curPay += (amount * curBet);
        curGamePay.text = MainConfig.CUR_PAY_STR + curPay.ToString();
    }

    private void ResetPay()
    {
        curPay = 0;
        curGamePay.text = MainConfig.CUR_PAY_STR + curPay.ToString();;
    }

    // Gets the symbol id from the display given in the response
    public int GetIDFromDisplay(List<List<string>> display, int colIdx, int rowIdx)
    {
        string symbolName = display[colIdx][rowIdx];
        int id = MainConfig.SYMBOL_TYPE_MAPPING[symbolName];
        return id;
    }

    public CascadeSymbol GetSymbolScript(GameObject symbolAnchor)
    {
        return symbolAnchor.transform.Find(MainConfig.SYMBOL_OBJECT).GetComponent<CascadeSymbol>();
    }

    public void ChangeSymbolTexture(GameObject symbolAnchor, int id)
    {
        CascadeSymbol cascadeSymbolScript = GetSymbolScript(symbolAnchor);
        cascadeSymbolScript.SetTextureID(id);
    }

    // logically cascades symbol instance grid e.g.
    // SymbolInstanceGrid[0] = [null, something1, null, something2, null]
    // it'd become [null, null, null, something1, something2]
    // repeat for rest of cols
    public void CascadeSymbolInstanceGrid()
    {
        for (int colIdx = 0; colIdx < cols; colIdx++)
        {
            int swapIdxPtr = rows - 1;
            for (int rowIdx = swapIdxPtr; rowIdx >= 0; rowIdx--)
            {
                GameObject symbolAnchor = symbolInstanceGrid[colIdx, rowIdx];
                if (symbolAnchor != null)
                {
                    // not null, let's move this symbolAnchor to the last row_idx_ptr
                    symbolInstanceGrid[colIdx, swapIdxPtr] = symbolAnchor;
                    if (rowIdx != swapIdxPtr)
                    {
                        symbolInstanceGrid[colIdx, rowIdx] = null;
                    }
                    swapIdxPtr--;
                }
            }
        }
    }

    // Syncs bet to the UI from bet controller
    public void SyncBet(int bet)
    {
        curBet = bet;
    }

}
