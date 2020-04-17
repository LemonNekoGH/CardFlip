using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameMain : MonoBehaviour
{
    public Button startButton;
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;
    public Button backButtonInSelect;
    public Button backButtonInOver;

    public Transform welcomePanel;
    public Transform selectLevelPanel;
    public Transform gamingPanel;
    public Transform gameOverPanel;

    public Transform cardPrefab; // 卡片预制件

    void Start()
    {
        startButton.onClick.AddListener(() =>
        {
            welcomePanel.gameObject.SetActive(false);
            selectLevelPanel.gameObject.SetActive(true);
        });
        backButtonInSelect.onClick.AddListener(() =>
        {
            selectLevelPanel.gameObject.SetActive(false);
            welcomePanel.gameObject.SetActive(true);
        });
        level1Button.onClick.AddListener(() => { GoToGamingPanel(1); });
        level2Button.onClick.AddListener(() => { GoToGamingPanel(2); });
        level3Button.onClick.AddListener(() => { GoToGamingPanel(3); });
        backButtonInOver.onClick.AddListener(() =>
        {
            gameOverPanel.gameObject.SetActive(false);
            selectLevelPanel.gameObject.SetActive(true);
        });
    }

    /// <summary>
    /// 进入关卡页面
    /// </summary>
    /// <param name="level"></param>
    private void GoToGamingPanel(int level)
    {
        selectLevelPanel.gameObject.SetActive(false);

        // 提前将牌准备好
        switch (level)
        {
            case 1:
                ShowCards(3, 2);
                break;
            case 2:
                ShowCards(4, 2);
                break;
            case 3:
                ShowCards(5, 2);
                break;
        }

        // 进入关卡
        gamingPanel.gameObject.SetActive(true);
    }

    // 打乱列表顺序
    private List<T> ListRandom<T>(List<T> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var index = Random.Range(0, list.Count - 1);
            if (i == index)
            {
                continue;
            }

            var temp = list[index];
            list[index] = list[i];
            list[i] = temp;
        }

        return list;
    }

    private void ShowCards(int width, int height)
    {
        var cardBack = Resources.LoadAll<Sprite>("CardBack")[0];
        var sprites = Resources.LoadAll<Sprite>("CardFront");
        var count = width * height / 2;
        var spriteList = sprites.ToList();
        var needShowList = new List<Sprite>();

        // 随机选择会出现的牌
        while (count > 0)
        {
            var rand = Random.Range(0, spriteList.Count);
            needShowList.Add(spriteList[rand]);
            needShowList.Add(spriteList[rand]);
            spriteList.RemoveAt(rand);
            count--;
        }

        needShowList = ListRandom(needShowList); // 打乱列表中牌的顺序

        var layoutPanel = gamingPanel.Find("Panel_Layout");

        needShowList.ForEach(sprite =>
        {
            var card = Instantiate(cardPrefab);
            var cardBackground = card.Find("Image_CardBack");
            var cardFront = card.Find("Image_CardFront");
            cardBackground.GetComponent<Image>().sprite = cardBack;
            cardFront.GetComponent<Image>().sprite = sprite;
            card.SetParent(layoutPanel); // 将新牌添加进去
        });

        var gridLayout = layoutPanel.GetComponent<GridLayoutGroup>();

        /*
         * 获取屏幕大小并设置padding
         */
        var panelWidth = width * gridLayout.cellSize.x + (width - 1) * gridLayout.spacing.x;
        var paddingHorizontal = (int) (Screen.width - panelWidth) / 2;
        var panelHeight = height * gridLayout.cellSize.y + (height - 1) * gridLayout.spacing.y;
        var paddingVertical = (int) (Screen.height - panelHeight) / 2;
        gridLayout.padding = new RectOffset(paddingHorizontal, paddingHorizontal, paddingVertical, paddingHorizontal);
    }

    /// <summary>
    /// 检查游戏是否满足结束条件
    /// </summary>
    public void CheckIsGameOver()
    {
        var cards = FindObjectsOfType<CardFlipAnimationCtrl>();
        var cardList = cards.ToList();
        var flippedCards = new List<CardFlipAnimationCtrl>();
        var twoFlippedCards = new List<CardFlipAnimationCtrl>();
        cardList.ForEach(card =>
        {
            if (!card.isFlipped || card.isMatched) return;
            twoFlippedCards.Add(card); // C#里的List添加时会把元素添加到所有元素的前面
        });
        if (twoFlippedCards.Count != 2) return;
        if (twoFlippedCards[0].cardFront.GetComponent<Image>().sprite.name ==
            twoFlippedCards[1].cardFront.GetComponent<Image>().sprite.name)
        {
            twoFlippedCards[0].MarkAsMatched();
            twoFlippedCards[1].MarkAsMatched();
            flippedCards.Add(twoFlippedCards[0]);
            flippedCards.Add(twoFlippedCards[1]);
        }
        else
        {
            twoFlippedCards[0].MarkAsMatchFailed();
            twoFlippedCards[1].MarkAsMatchFailed();
        }

        twoFlippedCards.Clear();

        var isWin = true;
        cardList.ForEach(card => { isWin &= card.isMatched; });
        if (isWin)
        {
            GameOver();
        }
    }

    /// <summary>
    /// 结束游戏
    /// </summary>
    private void GameOver()
    {
        welcomePanel.gameObject.SetActive(false);
        selectLevelPanel.gameObject.SetActive(false);
        gamingPanel.gameObject.SetActive(false);
        
        var layoutPanel = gamingPanel.Find("Panel_Layout");
        for (int i = 0; i < layoutPanel.childCount; i++)
        {
            Destroy(layoutPanel.GetChild(i).gameObject); // 销毁关卡中的牌，释放内存
        }
        
        gameOverPanel.gameObject.SetActive(true);
    }
}