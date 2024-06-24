using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using TMPro;
using DG.Tweening;

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
        public List<object> scatter_win_positions { get; set; }
    }

    public Sequence currentSeq;

    public TextMeshProUGUI testTextDeleteMe;

    public GameObject symbolPrefab;
    public int rows = 5;
    public int cols = 5;

    public float XOffset;
    public float YOffset;

    public float gridSpacing;

    public float gridZ;

    protected GameObject[,] symbolInstanceGrid;

    // Start is called before the first frame update
    void Start()
    {
        // instantiate the grid
        symbolInstanceGrid = new GameObject[cols, rows];

        for (int col_idx = 0; col_idx < cols; col_idx++)
        {
            for (int row_idx = 0; row_idx < rows; row_idx++)
            {
                GameObject symbolAnchor = Instantiate(symbolPrefab);
                // symbolAnchor.transform.position = new Vector3((i - XOffset) * gridSpacing, (j - YOffset) * gridSpacing, gridZ);
                symbolAnchor.transform.position = getIdleWorldPos(col_idx, row_idx);

                symbolInstanceGrid[col_idx, row_idx] = symbolAnchor;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void OnSpinClick()
    {
        StartCoroutine(GetRequest("http://54.66.230.103/api/data"));
    }

    public Vector3 RowColToWorldPos(int col_idx, int row_idx)
    {
        return Vector3.zero;
    }

    // from camera space
    // X is right
    // Y is up
    // Z is forward into screen

    public IEnumerator playSequence(Response responseObject)
    {
        Response resp = responseObject;

        float animDur = 1.0f;
        float delayDur = 1.0f;

        // general play sequence
        // Sequence mainSeq = DOTween.Sequence();
        // currentSeq = mainSeq;

        // TODO: refactor this into buildable pattern classful thingo

        for (int i = 0; i < resp.slot_data.Count; i++)
        {
            List<List<int>> cascade_positions = resp.slot_data[i].cascade_positions;
            string cur_mode = resp.slot_data[i].cur_mode;
            List<List<string>> display = resp.slot_data[i].display;
            string next_mode = resp.slot_data[i].next_mode;
            int pay = resp.slot_data[i].pay;
            int rounds_left = resp.slot_data[i].rounds_left;
            List<object> scatter_win_positions = resp.slot_data[i].scatter_win_positions;

            Sequence moveSeq = DOTween.Sequence();

            // setting the skippable thingo
            currentSeq = moveSeq;

            for (int col_idx = 0; col_idx < cols; col_idx++)
            {
                for (int row_idx = 0; row_idx < rows; row_idx++)
                {
                    // get the symbol anchor
                    GameObject symbolAnchor = symbolInstanceGrid[col_idx, row_idx];

                    // have to take a snapshot of the current idxs, can't use reference
                    int currentColIdx = col_idx;
                    int currentRowIdx = row_idx;

                    // Vector3 targetPos = new Vector3((i - XOffset) * gridSpacing - 1, (j - YOffset) * gridSpacing - 1, gridZ);
                    // Vector3 targetPos = new Vector3(symbolAnchor.transform.position.x - i - 1, symbolAnchor.transform.position.y - i - 1, symbolAnchor.transform.position.z);
                    // symbolAnchor.transform.DOMove(targetPos, 1).SetEase(Ease.InOutSine);
                    
   
                    // moveSeq.Join(symbolAnchor.transform.DOFade(0, 1).SetEase(Ease.InOutSine).SetEase(Ease.InOutSine));
                    // moveSeq.Join(symbolAnchor.transform.DOMove(targetPos, 2).SetEase(Ease.InOutSine));

                    Sequence subSeq = DOTween.Sequence();

                    if (cur_mode == "base")
                    {
                        // the symbol should move behind the background
                        Tween subTweenMoveBehind = symbolAnchor.transform.DOMove(symbolAnchor.transform.position + new Vector3(0, 0, 10), animDur).SetEase(Ease.InOutSine).OnComplete(() =>
                        {
                            // once in the background, change symbol texture

                            Debug.Log(display);
                            // foreach (var col in display)
                            // {
                            //     foreach (var symbolName in col)
                            //     {
                            //         Debug.Log(symbolName);
                            //     }
                            // }
                            // for (int i = 0; i < display.Count; i++)
                            // {
                            //     for (int j = 0; j < display[i].Count; j++)
                            //     {
                            //         Debug.Log(display[i][j]);
                            //     }
                            // }
                            // Debug.Log(display.Count);
                            // Debug.Log(display[0].Count);


                            string symbolName = display[currentColIdx][currentRowIdx];
                            int id = MainConfig.SYMBOL_TYPE_MAPPING[symbolName];
                            symbolAnchor.transform.Find("SymbolObject").GetComponent<GenericSymbol>().ChangeToID(id);
                            // snap it to drop point
                            symbolAnchor.transform.position = getIdleWorldPos(currentColIdx, currentRowIdx) + new Vector3(0, 0, -15);
                            Debug.Log(getIdleWorldPos(col_idx, row_idx));
                            Debug.Log(col_idx + ", " + row_idx);
                        });
                        subSeq.Join(subTweenMoveBehind);

                        // then in should tween down to resting pos
                        Tween subTweenLand = symbolAnchor.transform.DOMove(getIdleWorldPos(currentColIdx, currentRowIdx), animDur).SetEase(Ease.InOutSine);

                        // subSeq.Join(subTweenLand);
                        subSeq.Append(subTweenLand);
                    }   
                    else
                    {

                    }

                    moveSeq.Join(subSeq);
                }
            }

            // mainSeq.Append(moveSeq);

            yield return moveSeq.WaitForCompletion();

            // Debug.Log("waiting..." + i);
            // yield return new WaitForSeconds(animDur + delayDur);
        }

        yield return null;
    }

    public Vector3 getIdleWorldPos(int col_idx, int row_idx)
    {
        return new Vector3((col_idx - XOffset) * gridSpacing, (row_idx - YOffset) * gridSpacing, gridZ);
    }

    public void skipSequence()
    {
        // float roundedUpElapsed = roundUpToNearest(currentSeq.Elapsed(), 2);
        // Debug.Log(roundedUpElapsed);

        // currentSeq.Goto(roundedUpElapsed, true
        
        // );

        currentSeq.Complete();


        // stop all non tween anims as well (explosions/textures)

    }

    public float roundUpToNearest(float num, int multiple)
    {
        return Mathf.Ceil(num / multiple) * multiple;
    }

}
