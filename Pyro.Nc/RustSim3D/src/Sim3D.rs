use std::path::Iter;
use crate::CutResult::CutResult;
use crate::RustWrapper::RustWrapper;
use crate::Vector3::Vector3;

#[no_mangle]
pub extern "cdecl" fn check_position_for_cut(wrapper: &mut RustWrapper, throwIfCut: bool,
                                             func: extern "cdecl" fn(i: i32)) -> CutResult
{
    let mut result: CutResult = CutResult.new();
    let mut iter = wrapper.vertices.iter();
    let mut i = 0;
    let mut current = iter.next();
    
    while current != None 
    {
        let c = iter[i];
        
        func(i);
        current = iter.next();
        i += 1;
    }
    
    return result;
}