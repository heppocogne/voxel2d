extends InventorySlot



func _ready():
	pass


func _input_impl(event:InputEvent):
	if event is InputEventMouseButton:
		var mb:=event as InputEventMouseButton
		if mb.pressed:
			if Rect2(Vector2.ZERO,rect_size).has_point(get_local_mouse_position()):
				if mb.button_index==BUTTON_LEFT:
					if display_name=="":
						if gui_root.grabbed_item:
							# place
							pass
					else:
						var grabbed:GrabbedItem=preload("res://gui/inventory/grabbed_item.tscn").instance()
						grabbed.set_item_info(display_name,item_texture.texture,quantity)
						if gui_root.grabbed_item:
							pass
						else:
							# grab
							emit_signal("item_grabbed",grabbed)
							emit_signal("state_changed")
							clear()
							
				elif mb.button_index==BUTTON_RIGHT:
					if gui_root.grabbed_item:
						if display_name=="":
							set_item_info(gui_root.grabbed_item.display_name,gui_root.grabbed_item.texture,0)
						if gui_root.grabbed_item.display_name==display_name:
							# add an item
							pass
					else:
						# split
						var half:=quantity/2
						if 0<half:
							var grabbed:GrabbedItem=preload("res://gui/inventory/grabbed_item.tscn").instance()
							grabbed.set_item_info(display_name,item_texture.texture,quantity-half)
							emit_signal("item_grabbed",grabbed)
							set_item_quantity(half)
							emit_signal("state_changed")
