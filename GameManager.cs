using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

// MonoBehaviourを継承することでオブジェクトにコンポーネントとして
// アタッチすることができるようになる
public class GameManager : MonoBehaviour
{
    // SerializeFieldと書くとprivateなパラメーターでも
    // インスペクター上で値を変更できる
    [SerializeField]
    private Text mainText;
    [SerializeField]
    private Text nameText;
    // パラメーターを追加
    [SerializeField]
    private float captionSpeed = 0.2f;
    [SerializeField]
    private GameObject nextPageIcon;
    private const char SEPARATE_COMMAND = '!';
    private const char COMMAND_SEPARATE_PARAM = '=';
    private const string COMMAND_BACKGROUND = "background";
    private const string COMMAND_SPRITE = "_sprite";
    private const string COMMAND_COLOR = "_color";
    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private string spritesDirectory = "Sprites/";
    [SerializeField]
    private string textFile = "Texts/Scenario";
    private const string COMMAND_JUMP = "jump_to";



    // パラメーターを追加
    private const char SEPARATE_MAIN_START = '「';
    private const char SEPARATE_MAIN_END = '」';
    private Queue<char> _charQueue;
    private const char SEPARATE_PAGE = '&';
    private Queue<string> _pageQueue;

    private const string COMMAND_CHARACTER_IMAGE = "charaimg";
    private const string COMMAND_SIZE = "_size";
    private const string COMMAND_POSITION = "_pos";
    private const string COMMAND_ROTATION = "_rotate";
    private const string CHARACTER_IMAGE_PREFAB = "CharacterImage";
    [SerializeField]
    private GameObject characterImages;
    [SerializeField]
    private string prefabsDirectory = "Prefabs/";
    private List<Image> _charaImageList = new List<Image>();
    private const string COMMAND_ACTIVE = "_active";
    private const string COMMAND_DELETE = "_delete";
    private const char SEPARATE_SUBSCENE = '#';
    private Dictionary<string, Queue<string>> _subScenes =
           new Dictionary<string, Queue<string>>();
    private const string COMMAND_SELECT = "select";
    private const string COMMAND_TEXT = "_text";
    private const string SELECT_BUTTON_PREFAB = "SelectButton";
    [SerializeField]
    private GameObject selectButtons;
    private List<Button> _selectButtonList = new List<Button>();

    // パラメーターを変更
    private string _text = "";


    /**
* 選択肢がクリックされた
*/
    private void SelectButtonOnClick(string label)
    {
        foreach (Button button in _selectButtonList) Destroy(button.gameObject);
        _selectButtonList.Clear();
        JumpTo('"' + label + '"');
        ShowNextPage();
    }

    /**
* 選択肢の設定
*/
    private void SetSelectButton(string name, string cmd, string parameter)
    {
        cmd = cmd.Replace(COMMAND_SELECT, "");
        name = name.Substring(name.IndexOf('"') + 1, name.LastIndexOf('"') - name.IndexOf('"') - 1);
        Button button = _selectButtonList.Find(n => n.name == name);
        if (button == null)
        {
            button = Instantiate(Resources.Load<Button>(prefabsDirectory + SELECT_BUTTON_PREFAB), selectButtons.transform);
            button.name = name;
            button.onClick.AddListener(() => SelectButtonOnClick(name));
            _selectButtonList.Add(button);
        }
        SetImage(cmd, parameter, button.image);
    }

    /**
* 対応するラベルまでジャンプする
*/
    private void JumpTo(string parameter)
    {
        parameter = parameter.Substring(parameter.IndexOf('"') + 1, parameter.LastIndexOf('"') - parameter.IndexOf('"') - 1);
        _pageQueue = _subScenes[parameter];
    }

    /**
* パラメーターからboolを取得する
*/
    private bool ParameterToBool(string parameter)
    {
        string p = parameter.Replace(" ", "");
        return p.Equals("true") || p.Equals("TRUE");
    }

    /**
* テキストファイルを読み込む
*/
    private string LoadTextFile(string fname)
    {
        TextAsset textasset = Resources.Load<TextAsset>(fname);
        return textasset.text.Replace("\n", "").Replace("\r", "");
    }

    /**
* 立ち絵の設定
*/
    private void SetCharacterImage(string name, string cmd, string parameter)
    {
        cmd = cmd.Replace(COMMAND_CHARACTER_IMAGE, "");
        name = name.Substring(name.IndexOf('"') + 1, name.LastIndexOf('"') - name.IndexOf('"') - 1);
        Image image = _charaImageList.Find(n => n.name == name);
        if (image == null)
        {
            image = Instantiate(Resources.Load<Image>(prefabsDirectory + CHARACTER_IMAGE_PREFAB), characterImages.transform);
            image.name = name;
            _charaImageList.Add(image);
        }
        SetImage(cmd, parameter, image);
    }

    /**
    * パラメーターからベクトルを取得する
*/
    private Vector3 ParameterToVector3(string parameter)
    {
        string[] ps = parameter.Replace(" ", "").Split(',');
        return new Vector3(float.Parse(ps[0]), float.Parse(ps[1]), float.Parse(ps[2]));
    }


    /**
* スプライトをファイルから読み出し、インスタンス化する
*/
    private Sprite LoadSprite(string name)
    {
        return Instantiate(Resources.Load<Sprite>(spritesDirectory + name));
    }

    /**
    * パラメーターから色を作成する
*/
    private Color ParameterToColor(string parameter)
    {
        string[] ps = parameter.Replace(" ", "").Split(',');
        if (ps.Length > 3)
            return new Color32(byte.Parse(ps[0]), byte.Parse(ps[1]),
                                            byte.Parse(ps[2]), byte.Parse(ps[3]));
        else
            return new Color32(byte.Parse(ps[0]), byte.Parse(ps[1]),
                                            byte.Parse(ps[2]), 255);
    }

    /**
    * 画像の設定
*/
    private void SetImage(string cmd, string parameter, Image image)
    {
        cmd = cmd.Replace(" ", "");
        parameter = parameter.Substring(parameter.IndexOf('"') + 1, parameter.LastIndexOf('"') - parameter.IndexOf('"') - 1);
        switch (cmd)
        {
            case COMMAND_TEXT:
                image.GetComponentInChildren<Text>().text = parameter;
                break;
            case COMMAND_SPRITE:
                image.sprite = LoadSprite(parameter);
                break;
            case COMMAND_COLOR:
                image.color = ParameterToColor(parameter);
                break;
            case COMMAND_SIZE:
                image.GetComponent<RectTransform>().sizeDelta = ParameterToVector3(parameter);
                break;
            case COMMAND_POSITION:
                image.GetComponent<RectTransform>().anchoredPosition = ParameterToVector3(parameter);
                break;
            case COMMAND_ROTATION:
                image.GetComponent<RectTransform>().eulerAngles = ParameterToVector3(parameter);
                break;
            case COMMAND_ACTIVE:
                image.gameObject.SetActive(ParameterToBool(parameter));
                break;
            case COMMAND_DELETE:
                _charaImageList.Remove(image);
                Destroy(image.gameObject);
                break;
        }
    }


    /**
* 背景の設定
*/
    private void SetBackgroundImage(string cmd, string parameter)
{
   cmd = cmd.Replace(COMMAND_BACKGROUND, "");
   SetImage(cmd, parameter, backgroundImage);
}

    /**
* コマンドの読み出し
*/
    private void ReadCommand(string cmdLine)
    {
        cmdLine = cmdLine.Remove(0, 1);
        Queue<string> cmdQueue = SeparateString(cmdLine, SEPARATE_COMMAND);
        foreach (string cmd in cmdQueue)
        {
            string[] cmds = cmd.Split(COMMAND_SEPARATE_PARAM);
            if (cmds[0].Contains(COMMAND_BACKGROUND))
                SetBackgroundImage(cmds[0], cmds[1]);
            if (cmds[0].Contains(COMMAND_CHARACTER_IMAGE))
                SetCharacterImage(cmds[1], cmds[0], cmds[2]);
            if (cmds[0].Contains(COMMAND_JUMP))
                JumpTo(cmds[1]);
            if (cmds[0].Contains(COMMAND_SELECT))
                SetSelectButton(cmds[1], cmds[0], cmds[2]);
        }
    }

    /**
* 文字列を指定した区切り文字ごとに区切り、キューに格納したものを返す
*/
    private Queue<string> SeparateString(string str, char sep)
    {
        string[] strs = str.Split(sep);
        Queue<string> queue = new Queue<string>();
        foreach (string l in strs) queue.Enqueue(l);
        return queue;
    }

    /**
    * 次のページを表示する
*/
    private bool ShowNextPage()
    {
        if (_pageQueue.Count <= 0) return false;
        // オブジェクトの表示/非表示を設定する
        nextPageIcon.SetActive(false);
        ReadLine(_pageQueue.Dequeue());
        return true;
    }

    // メソッドを追加
    /**
    * 文を1文字ごとに区切り、キューに格納したものを返す
*/
    private Queue<char> SeparateString(string str)
    {
        // 文字列をchar型の配列にする = 1文字ごとに区切る
        char[] chars = str.ToCharArray();
        Queue<char> charQueue = new Queue<char>();
        // foreach文で配列charsに格納された文字を全て取り出し
        // キューに加える
        foreach (char c in chars) charQueue.Enqueue(c);
        return charQueue;
    }

    /**
* 1文字を出力する
*/
    private bool OutputChar()
    {
        if (_charQueue.Count <= 0)
        {
            nextPageIcon.SetActive(true);
            return false;
        }
        mainText.text += _charQueue.Dequeue();
        return true;
    }

    /**
* 文字送りするコルーチン
*/
    private IEnumerator ShowChars(float wait)
    {
        // OutputCharメソッドがfalseを返す(=キューが空になる)までループする
        while (OutputChar())
            // wait秒だけ待機
            yield return new WaitForSeconds(wait);
        // コルーチンを抜け出す
        yield break;
    }

    /**
* 全文を表示する
*/
    private void OutputAllChar()
    {
        StopCoroutine(ShowChars(captionSpeed));
        while (OutputChar()) ;
        nextPageIcon.SetActive(true);
    }

    /**
* クリックしたときの処理
*/
    private void OnClick()
    {
        if (_charQueue.Count > 0) OutputAllChar();
        else
        {
            if (_selectButtonList.Count > 0) return;
            if (!ShowNextPage())
                UnityEditor.EditorApplication.isPlaying = false;
        }
    }

    // MonoBehaviourを継承している場合限定で
    // 毎フレーム呼ばれる 
    private void Update()
    {
        // 左(=0)クリックされたらOnClickメソッドを呼び出し
        if (Input.GetMouseButtonDown(0)) OnClick();
    }

    // メソッドを変更
    private void ReadLine(string text)
    {
        if (text[0].Equals(SEPARATE_COMMAND))
        {
            ReadCommand(text);
            if (_selectButtonList.Count > 0) return;
            ShowNextPage();
            return;
        }
        string[] ts = text.Split(SEPARATE_MAIN_START);
        string name = ts[0];
        string main = ts[1].Remove(ts[1].LastIndexOf(SEPARATE_MAIN_END));
        if (name.Equals("")) nameText.transform.parent.gameObject.SetActive(false);
        else
        {
            nameText.text = name;
            nameText.transform.parent.gameObject.SetActive(true);
        }
        mainText.text = "";
        _charQueue = SeparateString(main);
        StartCoroutine(ShowChars(captionSpeed));
    }
    /**
* 初期化する
*/
    private void Init()
    {
        _text = LoadTextFile(textFile);
        Queue<string> subScenes = SeparateString(_text, SEPARATE_SUBSCENE);
        foreach (string subScene in subScenes)
        {
            if (subScene.Equals("")) continue;
            Queue<string> pages = SeparateString(subScene, SEPARATE_PAGE);
            _subScenes[pages.Dequeue()] = pages;
        }
        _pageQueue = _subScenes.First().Value;
        ShowNextPage();
    }

    private void Start()
    {
        Init();
    }



}
