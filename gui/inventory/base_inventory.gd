tool
extends NinePatchRect


func _ready():
	pass


func _on_MarginContainer_resized_or_sort_children():
	rect_min_size=$MarginContainer.rect_size
