# Arena 實驗計畫與審查紀錄

更新日期 2026-11-06。執行環境是隔離的 UnityTest2DProject 工作樹，使用 Unity `6000.0.31f1`。證據層級為 **Codex 本地回報**。GitHub 可獨立核對文件與此 PR 的變更範圍，執行資產和原始日誌仍在本地。從 `workflow_test_arena/main` 的乾淨 checkout 獨立重跑，尚未驗證。

## EXP-0015 雨夜候車亭

研究問題是小型 2D 場景能否在同一段流程中完成場景搭建、點擊互動、Ink 分支與可追溯驗證。

| ID | 已取得證據 | 待驗或限制 |
| --- | --- | --- |
| EXP015-T01 | 場景匯入並重開，12 個 SpriteRenderer 全部解析到實驗自有 Sprite。批次相機畫面可辨識五種佈景。玩家起點與兩個點擊區儲存後重開仍存在。 | 批次相機畫面與互動式 Game View 分開驗收。 |
| EXP015-T02 | 批次 PlayMode 檢查兩個 ID、按住和空白點擊。互動式 Editor 使用虛擬滑鼠，在正常 Update 驗證 `ticket-window → notice-board → ticket-window`。 | 互動式完整流程使用注入事件，實體滑鼠待驗。 |
| EXP015-T03 | Unity Ink 整合產生 JSON，批次驗證兩條路徑。互動式 Game View 的取票按鈕另收到 MouseDown 與 MouseUp，抵達藍燈結局。Unity 原生擷取顯示 1564×880 的繁中台詞、按鈕與結局。 | 畫面與原始日誌留在本地。其他解析度待驗。 |
| EXP015-T04 | 首次資產匯入約 631 秒。Unity API Updater 修正舊轉換器的命名空間。Ink 檢查需等待自動編譯完成。批次逐幀輸入曾停滯，改用手動推進 Input System 取得結果。 | 桌面擷取逾時，MCP Editor 狀態過期。一次匯入時間只代表該次環境。 |
| EXP015-T05 | 場景、互動、Ink 與紀錄已在執行環境分開提交。 | 本倉庫的審查分支只交付本文件。Unity Test Runner 未執行，以上來自批次檢查和 Editor 日誌。 |

互動式 Game View 驗證摘錄（Codex 本地回報）：

```text
EXP-0015 interaction: ticket-window
EXP-0015 interaction: notice-board
EXP-0015 interaction: ticket-window
ARENA_GAME_VIEW_BUTTON queued at (782.00, 699.60) on 1564x880
ARENA_GAME_VIEW_FULL PASS: ticket-window, notice-board, ticket-window
```

本地證據索引。T01 為批次匯入與相機渲染，T02 為逐步 PlayMode 日誌與互動式 Editor 的點擊日誌，T03 為場景到 Ink 的批次路徑日誌，T04 為首次匯入與重試紀錄，T05 為執行倉庫的 Git 紀錄。Ink 整合由執行專案內的 `Assets/Ink` 提供，Runtime 與編譯輸出格式均為 21。場景自有 Sprite GUID 為 `9b42944c04724da4a3404c63ad56ce3a`。

## EXP-0016　重跑一次最小場景驗證

**問題**　同一個工作樹能否用一個固定入口重跑 EXP-0015 的場景、輸入與 Ink 檢查，避免每次寫完即移除臨時探針而觸發重新編譯。

1. **E16-01 場景**　用指定 Unity 版本開啟實驗場景，檢查 12 筆 Sprite 引用、玩家起點與兩個點擊區。命令輸出 Unity 版本及來源提交。
2. **E16-02 輸入與故事**　重用原本通過的滑鼠狀態序列，檢查兩個 ID、按住和空白點擊。分別重跑兩條故事路徑。明列手動推進的步驟與人工畫面驗收的界線。
3. **E16-03 重跑成本**　在已匯入的同一工作樹連跑兩次，記錄每次耗時、退出碼與各檢查結果。第二次須產出相同結果，且不需修改場景、套件或手動介入。若批次初始化失敗，記錄具體階段及日誌定位。

本輪範圍限於可重跑的驗證入口，先沿用現有 Unity 與 Ink 設定。交付以本地執行證據與精簡 GitHub 摘要為準。是否納入正式流程，依兩次實測結果決定。

### 執行結果

兩次使用同一已匯入工作樹，場景基底為 `1bcce2f855f554657e948781892916da5b24202f`，runner 內容 SHA256 為 `04C46878ECEB31C55615F1E7690D7356EB25EEA43FCC9DC92EDF7B1CE58EB59E`。相同 runner 之後提交為本地 `95948cd`。執行入口為 Unity `-batchmode -nographics -accept-apiupdate -executeMethod RainyStopSmoke.Run`，並傳入工作樹、日誌路徑及 `ARENA_SMOKE_REVISION`。

| 執行 | 退出碼 | 耗時 | 結果 |
| --- | --- | --- | --- |
| 1 | 0 | 29.24 秒 | E16-01 場景、E16-02 先到窗口及先看公告均通過 |
| 2 | 0 | 20.08 秒 | 三項標記與第一次一致，E16-03 通過 |

兩次之間沒有人工介入，也沒有變更實驗場景、Ink 資產或套件。結果支持同工作樹的暖重跑。Unity Test Runner 執行零項。下一項重現性驗證是從執行倉庫的乾淨 checkout 跑同一入口，與 Game View 驗證分開記錄。

## 請審查

- 現有證據是否足以支持 EXP-0015 的場景、互動和 Ink 結論？哪些表述需要縮小？
- EXP-0016 的兩次重跑是否足以檢查可重複性？若要多做一項，哪項最能排除批次與互動式 Editor 的差異？
