class_name TileMapData
extends Resource

# id:PoolVector2Array
export var map:Dictionary


func _init():
    map={}


func get_rect()->Rect2:
    var rect:Rect2

    for id in map:
        for v in map[id]:
            rect.expand(v)
    
    return rect


func get_size()->Vector2:
    return get_rect().size
