VAR saw_notice = false
VAR arrived_with_signal = false

-> waiting_room

=== waiting_room ===
霧貼著窗沿，候船室只剩一扇窗亮著。遠處響了一聲短笛，末班渡船還沒有靠岸。
{ arrived_with_signal:
    船務員隔著玻璃朝你點頭：「訊號收到了，末班船會等你。」
}
{ saw_notice:
    你把東側浮橋的位置記在船票套背面。
}
+ [查看潮汐公告]
    -> notice_board
+ [走向售票窗]
    -> ticket_window
+ [留在候船室]
    你把濕透的外套掛在椅背，等下一次鳴笛。
    -> END

=== notice_board ===
潮汐公告寫著：「水位過低，末班船改靠東側浮橋。」
手繪箭頭從候船室一路指向碼頭東端。
~ saw_notice = true
-> waiting_room

=== ticket_window ===
船務員把手電筒擱在窗口旁，等你報上登船位置。
{ saw_notice or arrived_with_signal:
    { saw_notice:
        你說出東側浮橋。
    - else:
        船務員認出你在訊號亭送來的回訊，指向東側浮橋。
    }
    船務員點頭，把最後一張船票遞給你。
    + [取票，前往東側浮橋]
        -> boarded
- else:
    船務員說：「先看公告，今晚換了登船位置。」
    + [回去查看潮汐公告]
        -> notice_board
}

=== boarded ===
你踏上浮橋，聽見船鈴在霧裡響起。
渡船離岸。這段旅程結束。 # choice-only
+ [從候船室再玩一次]
    ~ saw_notice = false
    ~ arrived_with_signal = false
    -> waiting_room
