extends CenterContainer

signal selected_slot_changed(slot)

onready var item_slots:GridContainer=$GridContainer
onready var hotbar_slots:Array=$GridContainer.get_children()
var selected_slot:=0
var inventory_opened:=false


func _ready():
	pass


func _input(event:InputEvent):
	if inventory_opened:
		return
	
	if event is InputEventMouseButton:
		var mb:=event as InputEventMouseButton
		if mb.pressed:
			hotbar_slots[selected_slot].focused=false
			if mb.button_index==BUTTON_WHEEL_DOWN:
				selected_slot=((selected_slot+1)%9)
			if mb.button_index==BUTTON_WHEEL_UP:
				selected_slot=((selected_slot+8)%9)
			hotbar_slots[selected_slot].focused=true
			emit_signal("selected_slot_changed",selected_slot)
	
	elif event is InputEventKey:
		var key:=event as InputEventKey
		if key.pressed:
			if KEY_1<=key.scancode and key.scancode<=KEY_9:
				hotbar_slots[selected_slot].focused=false
				selected_slot=key.scancode-KEY_1
				hotbar_slots[selected_slot].focused=true
				emit_signal("selected_slot_changed",selected_slot)


func _on_PlayerInvetory_state_changed(items:Array):
	for i in range(9):
		var item=items[i]
		if item==null:
			hotbar_slots[i].clear()
		else:
			hotbar_slots[i].set_item_info(item.ItemName,item.ItemTexture,item.Quantity)


func _on_GUI_inventory_opened():
	inventory_opened=true
	mouse_filter=Control.MOUSE_FILTER_IGNORE


func _on_GUI_inventory_closed():
	inventory_opened=false
	mouse_filter=Control.MOUSE_FILTER_STOP
