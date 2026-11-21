VAR signal_sent = false

-> signal_booth

=== signal_booth ===
拉一下訊號桿，讓對岸知道你要搭末班渡船。 # await-world-click
-> DONE

=== notice_board ===
{ signal_sent:
    對岸亮起回訊燈。你收起訊號桿，往夜渡候船室走。 # depart-for-ferry
    -> END
- else:
    ~ signal_sent = true
    訊號已送出。聽到短笛後，再拉一次確認回訊。 # await-world-click
    -> DONE
}
