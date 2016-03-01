using At.Syntax;

namespace At {
    internal class CurlyBlockSyntax:ExpressionSyntax {
        private AtToken atToken;
        private string v;

        public CurlyBlockSyntax(string v,AtToken atToken) {
        this.v = v;
        this.atToken = atToken;
        }
    }
}