extends Control

export var player_nodepath:NodePath

signal inventory_opened()
signal inventory_closed()

onready var margin_container:Container=$MarginContainer
onready var vbox_container:Container=$MarginContainer/VBoxContainer
onready var hotbar:Container=$MarginContainer/VBoxContainer/VBoxContainer/Hotbar
onready var inventory_container:CenterContainer=$InventoryContainer
var inventory:BaseInventory
var grabbed_item:GrabbedItem


func _ready():
	get_tree().root.connect("size_changed", self, "_on_Window_size_changed")
	var player_node:KinematicBody2D=get_node(player_nodepath)
	player_node.get_node("Inventory").connect("StateChanged",hotbar,"_on_PlayerInvetory_state_changed")
	hotbar.connect("selected_slot_changed",player_node,"OnHotbarStateChanged")
	_on_Window_size_changed()


func _process(_delta:float):
	if Input.is_action_just_pressed("game_inventory"):
		if inventory:
			emit_signal("inventory_closed")
			inventory.queue_free()
			inventory=null
		else:
			emit_signal("inventory_opened")
			inventory=preload("res://gui/default_inventory/default_inventory.tscn").instance()
			inventory_container.add_child(inventory)


func _on_Window_size_changed():
	margin_container.rect_size=OS.window_size
	inventory_container.rect_size=OS.window_size


func _on_item_grabbed(grabbed:GrabbedItem):
	grabbed_item=grabbed
	add_child(grabbed_item)


func _on_item_released():
	if grabbed_item:
		grabbed_item.queue_free()
	grabbed_item=null
