const backendUri = `${window.location.protocol}//${window.location.hostname}:${window.location.port}`;
const getChallengeEndpoint = (provider) =>
    `${backendUri}/external-auth/challenge?provider=${provider}`;
let _handler;
function GetOAuthCode(provider) {
    function createOAuthMessageHandler(resolve, reject) {
        const handler = (event) => {
            if (event.data && event.data.type === 'oauth-result') {
                const data = event.data;
                if (data.code) {
                    resolve({
                        provider: data.provider,
                        code: data.code,
                    });
                } else {
                    reject({
                        provider: data.provider,
                        error: data.error,
                        errorDescription: data.errorDescription,
                    });
                }
                window.removeEventListener('message', handler, false);
            }
        };
        return handler;
    }

    return new Promise((resolve, reject) => {
        if (_handler) {
            window.removeEventListener('message', _handler, false);
        }
        _handler =createOAuthMessageHandler(resolve, reject); 
        window.addEventListener(
            'message',
            _handler,
            false,
        );
        window.open(
            getChallengeEndpoint(provider),
            undefined,
            'toolbar=no,menubar=no,directories=no,status=no,width=800,height=600',
        );
    });
}
