extends Control

signal selected_slot_changed(slot)

onready var margin_container:Container=$MarginContainer
onready var vbox_container:Container=$MarginContainer/VBoxContainer

var item_slots:Array
var selected_slot:=0


func _ready():
	get_tree().root.connect("size_changed", self, "_on_Window_size_changed")
	_on_Window_size_changed()
	item_slots=$MarginContainer/VBoxContainer/VBoxContainer/ItemSlots/Panel/GridContainer.get_children()


func _on_Window_size_changed():
	margin_container.rect_size=OS.window_size
	vbox_container.rect_size=OS.window_size


func _input(event:InputEvent):
	if event is InputEventMouseButton:
		var mb:=event as InputEventMouseButton
		if mb.pressed:
			item_slots[selected_slot].focused=false
			if mb.button_index==BUTTON_WHEEL_DOWN:
				selected_slot=((selected_slot+8)%9)
			if mb.button_index==BUTTON_WHEEL_UP:
				selected_slot=((selected_slot+1)%9)
			item_slots[selected_slot].focused=true
			emit_signal("selected_slot_changed",selected_slot)
	
	elif event is InputEventKey:
		var key:=event as InputEventKey
		if key.pressed:
			if KEY_1<=key.scancode and key.scancode<=KEY_9:
				item_slots[selected_slot].focused=false
				selected_slot=key.scancode-KEY_1
				item_slots[selected_slot].focused=true
				emit_signal("selected_slot_changed",selected_slot)
