tool
extends Button

signal craft_button_pressed(item,quantity,consumed)

var recipe:Dictionary


func _ready():
	pass


func set_recipe(r:Dictionary):
	recipe=r


func check_craftabe(items:Array):
	pass


func _on_HBoxContainer_resized_or_sort_children():
	rect_min_size=$HBoxContainer.rect_size


func _on_CraftButton_pressed():
	pass
