extends Control

signal inventory_opened()
signal inventory_closed()
signal screenshot_taken()

onready var margin_container:Container=$MarginContainer
onready var vbox_container:Container=$MarginContainer/VBoxContainer
onready var hotbar:Container=$MarginContainer/VBoxContainer/VBoxContainer/Hotbar
onready var inventory_container:CenterContainer=$InventoryContainer
var inventory:BaseInventory
var grabbed_item:GrabbedItem


func _ready():
	get_tree().root.connect("size_changed", self, "_on_Window_size_changed")
	_on_Window_size_changed()


func setup():
	var player_node:KinematicBody2D=$"../../Viewport/World/Player"
	player_node.get_node("Inventory").connect("StateChanged",hotbar,"_on_PlayerInvetory_state_changed")
	hotbar.connect("selected_slot_changed",player_node,"OnHotbarStateChanged")


func _process(_delta:float):
	if Input.is_action_just_pressed("game_inventory"):
		if inventory:
			emit_signal("inventory_closed")
			inventory.queue_free()
			inventory=null
		else:
			emit_signal("inventory_opened")
			inventory=preload("res://gui/inventory/default_inventory/default_inventory.tscn").instance()
			inventory_container.add_child(inventory)
	elif Input.is_action_just_pressed("game_pause"):
		var screenshot:=get_viewport().get_texture().get_data()
		screenshot.flip_y()
		emit_signal("screenshot_taken",screenshot)
		add_child(preload("res://gui/pause_menu/pause_menu.tscn").instance())
		get_tree().paused=true


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
