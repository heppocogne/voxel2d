extends VBoxContainer

onready var item_slots:Array


func _ready():
	item_slots=[]
	for s in $GridContainer2.get_children():
		item_slots.push_back(s)
	for s in $GridContainer.get_children():
		item_slots.push_back(s)


func _on_PlayerInvetory_state_changed(items:Array):
	for i in range(36):
		var item=items[i]
		if item==null:
			item_slots[i].clear()
		else:
			item_slots[i].set_item_info(item.ItemName,item.ItemTexture,item.Quantity)
