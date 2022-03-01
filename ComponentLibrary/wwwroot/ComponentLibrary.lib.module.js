// both of these initializers run before their caller project 
//so we can call scripts that we need to call in each phase
// They run even in pages that they are not used so we should be carefull
export function beforeStart(options,extensions) {
    console.log("Befor start initializer in RCL");
}
export function afterStarted(blazor) {
    console.log("After started initializer in RCL");
    addEventListener("catdog",e=>{console.log("This event listener added on RCL and should not be runned on none RCLs (in case of hard refresh I think)??",e)});
    // console.log(blazor);
}